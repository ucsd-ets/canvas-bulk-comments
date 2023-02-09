using System; 
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Threading.Tasks;

using BulkComments;
using CsvHelper;
using Wayfinder;
using Wayfinder.Canvas;

public partial class SubmissionComments : Page {

OauthClient Client;
LTIResponse LTI;

private void Page_Load(object sender, System.EventArgs e)
	{
	LTI = Session["SubmissionComments.LtiResponse"] as LTIResponse;
	if(LTI == null)
		{
		Unauthenticated.Visible = true;
		Debug.Text = "No LTI session found.";
		return;
		}

	string username = LTI.Username;

	try {
		if(String.IsNullOrEmpty(username))
			{
			Unauthenticated.Visible = true;
			Debug.Text = "No username found.";
			return;
			}

		var oauthRepo = new OauthTokenRepo();
		Client = new OauthClient(username, oauthRepo);

		var token = Client.Token;

		if(!(String.IsNullOrEmpty(Request.QueryString["code"])))
			{
			token = Client.AuthorizeToken(Request.QueryString["state"], Request.QueryString["code"], 
				ConfigurationManager.AppSettings["LTI.SubmissionComments.Dest"]);
			}

		if(token == null || String.IsNullOrEmpty(token.username))
			{
			Debug.Text = String.Format("Unable to figure out your Canvas username! Please contact us to figure out what went wrong. {0}", token == null ? "Token Null" : "Token Not Null");
			return;
			}

		if(Client.NeedsAuthorization)
			{
			string oauthInitUrl = Client.AuthorizationUrl(Convert.ToString(Request.Url));
			Debug.Text += "<br>" +oauthInitUrl;
			Response.Redirect(oauthInitUrl);
			return;
			}

		Authenticated.Visible = true;
		}
	catch(Exception except)
		{
		Debug.Text += String.Format("An exception occurred during setup! {0}", except.Message);
		return;
		}

	if(Client == null) 
		{
		Debug.Text += String.Format("No client found! Not loading courses. ");
		return;
		}

	if (!IsPostBack)
		{
		LoadAssignmentList();
		}
	}

protected void ShowAssignment(object sender, CommandEventArgs args)
	{
	if(!CommentSpreadsheet.HasFile)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">Please select the CSV file with your students and comments.</p>");
		return;
		}

	try {
		var csv = StudentCSVFactory.LoadStudentCSV(CommentSpreadsheet.FileContent);
		var matcher = new StudentAssignmentMatcher {CSV=csv};

		var matched_submissions = new List<SubmissionCommentMatch>();
		var unmatched_submissions = new List<Wayfinder.Canvas.Submission>();

		var cr = new Wayfinder.Canvas.CourseRepo(Client);
		var submissions = cr.GetAssignmentSubmissions(Convert.ToInt32(Assignments.SelectedItem.Value), Convert.ToInt32(LTI.CourseID)).Result;

		foreach(var submission in submissions)
			{
			var comment = matcher.MatchUser(submission.user);

			matched_submissions.Add(new SubmissionCommentMatch
				{
				Submission = submission,
				MatchedComment = comment
				});

			if(comment == null)
				{
				unmatched_submissions.Add(submission);
				}
			}

		Submissions.DataSource = matched_submissions;
		Submissions.DataBind();

		SelectAssignmentPanel.Visible = false;
		ReviewCommentsPanel.Visible = true;
		}
	catch(NoIdentifierException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">{0}</p>", except.Message);
		}
		
	catch(CsvHelper.BadDataException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">Uploaded CSV {0} had no valid data - did you upload a CSV? XLS/XLSX files are not supported.</p>", CommentSpreadsheet.FileName);
		}
	catch(NoDataException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">Uploaded CSV {0} had no data.</p>", CommentSpreadsheet.FileName);
		}
	catch(DuplicateIdentifierException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">{0}</p>", except.Message);
		}
	catch(DuplicateColumnException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">{0}</p>", except.Message);
		}
	catch(NoIdentifyingFieldException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">Could not find any fields containing student identifiers in the uploaded spreadsheet.</p>");
		}
	catch(NoCommentFieldException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">Could not find a field containing comments to import.</p>");
		}
	catch(DuplicateMatchException except)
		{
		PreviewFeedback.Text = String.Format("<p class=\"alert alert-danger\">Multiple rows in the spreadsheet match the same student.</p>");
		}
	/*
	catch(Exception except)
		{
		Debug.Text += String.Format("An error occurred trying to load submissions for assignment {1}! {0}<br/>", except.Message, Assignments.SelectedItem.Value);
		}
	*/
	}

protected void AddComments(object sender, CommandEventArgs args)
	{
	AddDebug.Text = "";

	int course_id, assignment_id;
	Wayfinder.Canvas.CourseRepo cr = null;

	try {
		course_id = Convert.ToInt32(LTI.CourseID);
		assignment_id = Convert.ToInt32(Assignments.SelectedItem.Value);

		cr = new Wayfinder.Canvas.CourseRepo(Client);
		}
	catch(Exception except)
		{
		AddDebug.Text = "<p class=\"alert alert-danger\">Could not prepare insertion of course comments: " + except.Message + "</p>";
		return;
		}

	ReviewCommentsPanel.Visible = false;
	AddResults.Text = "";
	ConfirmationPanel.Visible = true;

	var errors = new List<string>();
	var warnings = new List<string>();

	int updated = 0;

	var comments = new List<UpdateGrades>();

	foreach(DataGridItem item in Submissions.Items)
		{
		string display_name = "";
		try {
			int user_id = Convert.ToInt32(((Label) item.FindControl("UserID")).Text);
			string name = ((Label) item.FindControl("UserFullName")).Text;
			string pid = ((Label) item.FindControl("UserPID")).Text;

			string comment = ((TextBox) item.FindControl("SubmissionComment")).Text;

			display_name = String.Format("{0} [{1}]", name, pid);

			if(String.IsNullOrEmpty(comment))
				warnings.Add(String.Format("Skipped {0} - no comment was entered.", display_name));
			else
				{
				comments.Add(new UpdateGrades { user_id=user_id, text_comment=comment });
				updated++;
				}
			}
		catch(Exception except)
			{
			errors.Add(String.Format("Failed to add comment to {0}: {1}", display_name, except.Message));
			}
		}


	var progress = cr.AddCommentsToSubmission(assignment_id, course_id, comments).Result;

	if(progress == null)
		{
		errors.Add("A failure occurred while trying to send these comments to Canvas. Please send the spreadsheet to EdTech@ucsd.edu so that we may investigate why this error occurred.");
		}
	else
		{
		if(updated > 0)
			AddResults.Text += "<p class=\"alert alert-success\">Uploaded comments for " + updated + " students. They should start appearing in the gradebook over the next several seconds.</p>";
		}

	if(warnings.Count > 0)
		{
		AddResults.Text += "<div class=\"alert alert-warning\">Warning: <ul><li>" + String.Join("</li><li>", warnings) + "</li></ul></div>";
		}

	if(errors.Count > 0)
		{
		AddResults.Text += "<div class=\"alert alert-danger\">Some errors were encountered: <ul><li>" + String.Join("</li><li>", errors) + "</li></ul></div>";
		}

	}
private void LoadAssignmentList()
	{
	try {
		var cr = new Wayfinder.Canvas.CourseRepo(Client);
		var course = cr.GetCourse(Convert.ToInt32(LTI.CourseID)).Result;

		CourseName.Text = course.name;

		var assignments = cr.GetCourseAssignments(course.id).Result;

		var published = assignments.Where(a => a.published).OrderBy(a => a.name).ThenBy(a => a.id);

		Assignments.DataSource = published;
		Assignments.DataBind();
		}
	catch(Exception except)
		{
		Debug.Text += String.Format("An error occurred trying to load a list of the course's assignments! {0}", except.Message);
		return;
		}

	}
}

public class SubmissionCommentMatch {
public Wayfinder.Canvas.Submission Submission {get; set;}
public BulkComments.StudentComment MatchedComment {get; set;}

public bool HasComment 
	{ 
	get { 
		return MatchedComment != null; 
		}
	}

public string Comment { 
	get { 
		if(!HasComment) return null;

		return MatchedComment.Comment;
		}
	}
}
