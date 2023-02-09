using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wayfinder.Canvas;

namespace BulkComments {

public class StudentAssignmentMatcher {

private StudentCSV _CSV;
public StudentCSV CSV 
	{
	get { 
		return _CSV; 
		}
	set { 
		_CSV = value; 
		InitializeComparer();
		}
	}

public Dictionary<int, StudentCSVRow> CanvasIDLookup;
public Dictionary<string, StudentCSVRow> PIDLookup;
public Dictionary<string, StudentCSVRow> UsernameLookup;

public string CommentField = null;

private void InitializeComparer()
	{
	// Determine fields to use
	Regex CanvasIdPattern = new Regex("^(Canvas[ -])?ID$", RegexOptions.IgnoreCase);
	Regex PIDPattern = new Regex("^(SIS User ID|PID|Student ID)$", RegexOptions.IgnoreCase);
	Regex UsernamePattern = new Regex("^(SIS Login ID|username)$", RegexOptions.IgnoreCase);
	Regex CommentPattern = new Regex("^(assignment[ _])?comments?", RegexOptions.IgnoreCase);
	
	string CanvasID = null;
	string PID = null;
	string Username = null;
	CommentField = null;

	foreach(string field in CSV.Fields)
		{
		if(CanvasIdPattern.IsMatch(field))
			{
			if(CanvasID != null) throw new DuplicateColumnException(
				String.Format("CSV has at least two possible Canvas ID fields: '{0}' and '{1}'.", 
					CanvasID, field));
			CanvasID = field;
			}

		else if(PIDPattern.IsMatch(field))
			{
			if(PID != null) throw new DuplicateColumnException(
				String.Format("CSV has at least two possible PID fields: '{0}' and '{1}'.", 
					PID, field));
			PID = field;
			}

		else if(UsernamePattern.IsMatch(field))
			{
			if(Username != null) throw new DuplicateColumnException(
				String.Format("CSV has at least two possible username fields: '{0}' and '{1}'.", 
					Username, field));
			Username = field;
			}

		else if(CommentPattern.IsMatch(field))
			{
			if(CommentField != null) throw new DuplicateColumnException(
				String.Format("CSV has at least two possible comment fields: '{0}' and '{1}'.", 
					CommentField, field));
			CommentField = field;
			}
		}
	
	if(CommentField == null)
		throw new NoCommentFieldException();
	if(CanvasID == null && PID == null && Username == null)
		throw new NoIdentifyingFieldException();

	CanvasIDLookup = new Dictionary<int, StudentCSVRow>();
	PIDLookup = new Dictionary<string, StudentCSVRow>();
	UsernameLookup = new Dictionary<string, StudentCSVRow>();

	int index = 1;
	foreach(var row in CSV.Data)
		{
		index++;
		// Skip rows with no comment
		if(!row.Data.ContainsKey(CommentField) || String.IsNullOrEmpty(row.Data[CommentField])) continue;

		int user_id = -1;
		bool mapped = false;
		if(CanvasID != null && Int32.TryParse(row.Data[CanvasID], out user_id))
			{
			if(CanvasIDLookup.ContainsKey(user_id)) 
				throw new DuplicateIdentifierException(String.Format("Duplicate Canvas ID: {0} found for multiple students under the column '{1}'.", user_id, CanvasID));
			CanvasIDLookup[user_id] = row;
			mapped = true;
			}

		if(PID != null && !String.IsNullOrEmpty(row.Data[PID]))
			{
			if(PIDLookup.ContainsKey(row.Data[PID])) 
				throw new DuplicateIdentifierException(String.Format("Duplicate Student ID: {0} found for multiple students under the column '{1}',", row.Data[PID], PID));
			PIDLookup[row.Data[PID]] = row;
			mapped = true;
			}

		if(Username != null && !String.IsNullOrEmpty(row.Data[Username]))
			{
			if(UsernameLookup.ContainsKey(row.Data[Username])) 
				throw new DuplicateIdentifierException(String.Format("Duplicate Username: {0} found for multiple students under the column '{1}'.", row.Data[Username], Username));
			UsernameLookup[row.Data[Username]] = row;
			mapped = true;
			}

		if(!mapped)
			throw new NoIdentifierException(String.Format("No user identifier was found for row {0}, with the comment '{1}'", index, row.Data[CommentField]));
		}
	}

public StudentComment MatchUser(Wayfinder.Canvas.User user)
	{
	StudentCSVRow match = null;
	if(CanvasIDLookup.ContainsKey(user.id))
		match = CanvasIDLookup[user.id];

	if(!String.IsNullOrEmpty(user.sis_user_id) && PIDLookup.ContainsKey(user.sis_user_id))
		{
		var pidMatch = PIDLookup[user.sis_user_id];
		if(match != null && pidMatch != match) throw new DuplicateMatchException();
		match = pidMatch;
		}
	if(!String.IsNullOrEmpty(user.login_id) && UsernameLookup.ContainsKey(user.login_id))
		{
		var loginMatch = UsernameLookup[user.login_id];
		if(match != null && loginMatch != match) throw new DuplicateMatchException();
		match = loginMatch;
		}

	if(match == null)
		return null;

	return new StudentComment{Row=match, CommentField=CommentField};
	}
}
}
