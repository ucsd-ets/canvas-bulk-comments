using System;
using System.Web.UI;

public partial class Landing : Page {

protected string Message;

private void Page_Load(object sender, System.EventArgs e) 
	{
	LTIResponse response = LTIHelper.CheckRequest(Request, "SubmissionComments");
	LTIHelper.Log(response);

	if(!response.Authenticated)
		{
		Message += String.Format("<p>LTI authentication failed: {0}", response.UnauthenticatedMessage);
		}
	else
		{
		Message += String.Format("<p>User is authenticated: <a href='{0}'>{1} - {2} - {3}</a></p>", response.Destination, response.Username, response.CourseID, response.Role);

		Session["SubmissionComments.LtiResponse"] = response;

		if(Request.QueryString["debug"] == null) Response.Redirect("Default.aspx");
		}
	DataBind();
	}

}
