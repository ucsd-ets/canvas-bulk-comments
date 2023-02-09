using System;        
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Security.Cryptography;
using System.Text;

/* From bin/LtiLibrary.AspNet.dll, requires LtiLibrary.Core.dll and NewtonSoft.Json.dll */
using LtiLibrary.AspNet.Extensions;

public class LTIHelper  {

public static void Log(LTIResponse response)
	{
	// TODO add your logging code here
	}

public static LTIResponse CheckRequest(HttpRequest Request, string app = "eGrades") 
	{ 
	LTIResponse response = new LTIResponse { App=app };
	var RequestB = Request.RequestContext.HttpContext.Request;

	List<string> errors = new List<string>();

	var LmsName = "The LMS";
	var ToolName = "The TA Tool Link";
	var CourseIdField = "context_label";

	if(RequestB["tool_consumer_info_product_family_code"] == "canvas")
		{
		LmsName = "Canvas";
		ToolName = "TA Add Tool";
		CourseIdField = "custom_canvas_course_id";

		response.LMS = RequestB["custom_canvas_api_domain"];
		if(RequestB["custom_canvas_api_domain"] == "ucsd.test.instructure.com")
			{
			response.Dev = true;
			}
		}

	/* Make sure the request is not stale */

	if (String.IsNullOrEmpty(RequestB.Form["oauth_timestamp"])) 
		errors.Add(String.Format("{0} did not supply a request timestamp", LmsName));

	long oauthTimestamp = Convert.ToInt64(RequestB.Form["oauth_timestamp"]);
	long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
	long age = currentTimestamp - oauthTimestamp;

	if(age > 60 || age < -300)
		errors.Add(String.Format("Request is stale ({0} seconds old) - perhaps you used the back button and then the forward button?", age));

	/* Verify the request signature */
	string sentSignature = RequestB["oauth_signature"];
	if (String.IsNullOrEmpty(sentSignature)) 
		errors.Add(String.Format("{0} did not include a signature to verify your identity", LmsName));
	else
		{
		string correctSignature = RequestB.GenerateOAuthSignature(ConfigurationManager.AppSettings["LTI."+ app +".Key"]);
		if (sentSignature != correctSignature)
			errors.Add(String.Format("Received an invalid message signature from {2}! Received {0}; generated {1}. Not Trusted", sentSignature, correctSignature, LmsName));
		}

	/* Check the username */
	if(LmsName == "Canvas")
		{
		// Canvas supplies the username directly
		response.Username = RequestB["custom_canvas_user_login_id"];
		} 
	else
		{
		// Blackboard: Split email address for username
		string email = RequestB.Form["lis_person_contact_email_primary"];
		if (String.IsNullOrEmpty(email)) 
			errors.Add(String.Format("{0} did not include your email address", LmsName));
		else
			{
			string[] parts = email.Split(new Char[] { '@' });

			if (parts.Length > 0)
				{
				response.Username = parts[0];
				}
			}
		}

	if (String.IsNullOrEmpty(response.Username)) 
		errors.Add(String.Format("{0} did not include your username", LmsName));

	/* Add in the user's last name */
	response.Name = RequestB.Form["lis_person_name_full"];

	/* Check the sis_course_id */
	response.CourseID = RequestB.Form[CourseIdField];
	if (String.IsNullOrEmpty(response.CourseID)) 
		errors.Add(String.Format("This tool only works for courses with a CourseID. {0} did not supply one", LmsName));

	/* Verify that a valid role was included */
	string roles = RequestB.Form["roles"];

	if (String.IsNullOrEmpty(roles)) 
		errors.Add(String.Format("{0} did not convey your role in the course", LmsName));
	else
		{
		var allowedRoles = ConfigurationManager.AppSettings["LTI."+ app +".InstructorRoles"].Split(new Char[] { ',' });
		string[] roleParts = RequestB.Form["roles"].Split(new char[] { ',' });

		foreach (string found_role in roleParts)
			{
			if (allowedRoles.Contains(found_role))
				{
				response.Role = "INSTRUCTOR";
				break;
				}
			}
		}

	if (String.IsNullOrEmpty(response.Role))
		{
		errors.Add("This tool is only available to course instructors");
		if(!String.IsNullOrEmpty(RequestB.Form["roles"]))
			errors.Add(String.Format("you have roles {0}", RequestB.Form["roles"]));
		}

	if (errors.Count > 0)
		{
		response.UnauthenticatedMessage = String.Join("; ", errors) + 
			String.Format(". Please return to {0} in {1} and try again, or contact support if the problem persists.", ToolName, LmsName);

		return response;
		}

	/* We're good! */
	response.Authenticated = true;
	return response;
	}
}

[Serializable()]
public class LTIResponse
	{
	public string Username {get; set;}
	public string Name {get; set;}
	public string CourseID {get; set;}
	public string Role     {get; set;}
	public string App      {get; set;}
	public string LMS      {get; set;}

	public bool Authenticated { get; set; }
	public bool Dev { get; set; }

	public string UnauthenticatedMessage { get; set; }

	public string BaseURL {
		get {
			return ConfigurationManager.AppSettings["LTI." + App + ".Dest"];
			}
		}

	public string Destination {
		get {
			return String.Format("{0}?course_id={1}", BaseURL, CourseID);
			}
		}
	}
