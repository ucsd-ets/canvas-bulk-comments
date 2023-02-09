<%@ Page Language="C#" Inherits="SubmissionComments" CodeFile="Default.aspx.cs"  Title="Course Finder - UC San Diego" %>
<!doctype html>
<html lang="en">
<head>
	<title>Submission Comments Bulk Loader</title>
	<link rel="stylesheet" type="text/css" href="/css/bootstrap.css" media="screen" />
	<link rel="stylesheet" type="text/css" href="/css/wayfinder.css" media="screen" />
	<link rel="stylesheet" type="text/css" href="/css/SubmissionComments.css" media="screen" />

	<style>
	body {
		padding: 1em;
	}
	.alert-info {
		color: black;
		background-color: #eee;
		border: 1px solid #ccc;
		}

	.table > tbody > tr > th
		{
		border-top: 1px solid rgb(128,128,128);
		}
	.help
		{
		font-style: italic;
		}
	</style>
</head>
<body>

<h1>Submission Comments Bulk Loader</h1>
<p><asp:Label runat="server" id="Debug" /></p>

<asp:Panel runat="server" id="Unauthenticated" CssClass="alert alert-danger" visible="false">
<h2>Return to Canvas</h2>
<div>This tool must be accessed from Canvas. Please <a href="https://canvas.ucsd.edu/" target="_parent">return to canvas</a> and click the link in your Canvas course.</div>
</asp:Panel>

<asp:Panel runat="server" id="Authenticated" visible="false">
	<form runat="server">

	<asp:Panel runat="server" id="SelectAssignmentPanel">
	<h2>Assignments in '<asp:Literal runat="server" id="CourseName" />'</h2>

	<p><asp:Label runat="server" associatedControlId="Assignments">Add comments to the assignment</asp:Label>: <asp:DropDownList runat="server" id="Assignments" DataTextField="name" DataValueField="id" AutoPostBack="false"/> <span class="help">(Only published assignments are shown).</span></p>

	<div class="alert alert-info">
		<p>The easiest way to prepare a spreadsheet is to export your gradebook from Canvas, delete all the columns except 'ID', 'SIS User ID', and 'SIS Login ID', add a column named 'Comments', and enter your comments in that column.</p>

		<p>If you create a spreadsheet yourself:</p>

		<ul>
			<li>Include column labels in the first row</li>
			<li>Include at least one of the following columns to identify students:
				<ul>
					<li>"ID" or "Canvas ID": The Canvas numerical identifier, found in the "ID" column of a gradebook export
					<li>"SIS User ID" or "PID": The student's A-number
					<li>"SIS Login ID" or "username": The student's campus username, as used to log in to Canvas
				</ul>
			</li>
			<li>Include a column of comments named "Comment", "Comments", or "Assignment Comments"</li>
			<li>Each row with a comment must have a value for at least one user-identifying column that corresponds to a student in this class.</li>
			<li>Any comments without a user identifier will be ignored.</li>
			<li>If more than one user-identifying column has a value, each value must refer to the same student.</li>
		</ul>
	</div>


	<p>
	<asp:Label runat="server" associatedControlId="CommentSpreadsheet">Load comments from a .CSV spreadsheet</asp:Label>: <asp:FileUpload id="CommentSpreadsheet" runat="server" />
	</p>

	<asp:Literal runat="server" id="PreviewFeedback" />
	<p>
	<asp:Button id="ShowAssignmentButton" runat="server" OnCommand="ShowAssignment" Text="Preview Assignment Comments" CssClass="btn btn-primary" /> <span class="help">(Large classes may take up to a minute to load)</span>
	</p>
	<script>
	const button = document.getElementById("ShowAssignmentButton");
	button.form.addEventListener("submit", function() {
		button.value = "Please Wait...";
		window.setTimeout(function() { button.disabled = "true"; }, 25);
		});
	</script>
	</asp:Panel>

	<asp:Panel runat="server" id="ReviewCommentsPanel" visible="false">
	<p class="alert alert-success">Review the comments loaded from your spreadsheet below. Make any necessary modifications, and click <strong>Save Comments</strong> to finish. Blank comments will be skipped.</p>
	<asp:DataGrid runat="server" 
		id="Submissions" CssClass="table table-striped" UseAccessibleHeader="true" 
		AutoGenerateColumns="false"
		>
	<Columns>
		<asp:TemplateColumn HeaderText="user_id" visible="false"><ItemTemplate><asp:Label id="UserID" runat="server" text="<%# ((SubmissionCommentMatch) Container.DataItem).Submission.user.id %>" /></ItemTemplate></asp:TemplateColumn>
		<asp:TemplateColumn HeaderText="login_id" visible="false"><ItemTemplate><%# ((SubmissionCommentMatch) Container.DataItem).Submission.user.login_id %></ItemTemplate></asp:TemplateColumn>

		<asp:TemplateColumn HeaderText="Student"><ItemTemplate><asp:Label id="UserFullName" runat="server" text="<%# ((SubmissionCommentMatch) Container.DataItem).Submission.user.name %>" /></ItemTemplate></asp:TemplateColumn>
		<asp:TemplateColumn HeaderText="PID"><ItemTemplate><asp:Label id="UserPID" runat="server" text="<%# ((SubmissionCommentMatch) Container.DataItem).Submission.user.sis_user_id %>" /></ItemTemplate></asp:TemplateColumn>

		<asp:TemplateColumn HeaderText="Submission">
		<ItemTemplate>
			ID: <%# ((SubmissionCommentMatch) Container.DataItem).Submission.id %>
			<asp:panel runat="server" visible=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.attempt != null %>>
				Attempt: <%# ((SubmissionCommentMatch) Container.DataItem).Submission.attempt %>
			</asp:Panel>
			<asp:panel runat="server" visible=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.score != null %>>
				Score: <%# ((SubmissionCommentMatch) Container.DataItem).Submission.score %>
			</asp:Panel>
			<asp:panel runat="server" visible=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.grade != null %>>
				Grade: <%# ((SubmissionCommentMatch) Container.DataItem).Submission.grade %>
			</asp:Panel>
			<asp:panel runat="server" visible=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.body != null %>>
				Body: <%# ((SubmissionCommentMatch) Container.DataItem).Submission.body %>
			</asp:Panel>
			<asp:panel runat="server" visible=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.url != null %>>
				<a href="<%# ((SubmissionCommentMatch) Container.DataItem).Submission.url %>" target="_blank">Open</a>
			</asp:Panel>
		</ItemTemplate>
		</asp:TemplateColumn>

		<asp:TemplateColumn HeaderText="Previous Comments">
		<ItemTemplate>
			<asp:Panel runat="server" visible=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.submission_comments.Count > 0 %>>
				<ul class="list-group" >
				<asp:Repeater runat="server" DataSource=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.submission_comments%>>
				<ItemTemplate>
					<li class="list-group-item">
						<blockquote class="blockquote">
						<%# ((Wayfinder.Canvas.SubmissionComment) Container.DataItem).comment %>
						<footer class="blockquote-footer"><%# ((Wayfinder.Canvas.SubmissionComment) Container.DataItem).author_name %>, <%# ((Wayfinder.Canvas.SubmissionComment) Container.DataItem).created_at %></footer>
						</blockquote>
					</li>
				</ItemTemplate>
				</asp:Repeater>
				</ul>
			</asp:Panel>
			<asp:Panel runat="server" visible=<%# ((SubmissionCommentMatch) Container.DataItem).Submission.submission_comments.Count == 0 %>>
				<span class="help">None</span>
			</asp:Panel>

		</ItemTemplate>
		</asp:TemplateColumn>
		<asp:TemplateColumn HeaderText="New Comment">
		<ItemTemplate>
				<asp:Panel runat="server" visible=<%# !((SubmissionCommentMatch) Container.DataItem).HasComment %> CssClass="help">
					This student was not found in your spreadsheet.
				</asp:Panel>

			<asp:TextBox id="SubmissionComment" runat="server" rows="4" columns="40" TextMode="MultiLine" Text=<%# ((SubmissionCommentMatch) Container.DataItem).Comment %> />
		</ItemTemplate>
		</asp:TemplateColumn>

	</Columns>
	</asp:DataGrid>

	<p><asp:Label runat="server" id="AddDebug" /></p>
	<asp:Button runat="server" OnCommand="AddComments" Text="Add Comments to Students" CssClass="btn btn-primary" />
	</asp:Panel>

	<asp:Panel runat="server" id="ConfirmationPanel" visible="false">
	<asp:Literal runat="server" id="AddResults" />
	</asp:Panel>

	</form>
</asp:Panel>

</body>
</html>
