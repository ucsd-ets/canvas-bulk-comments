using System;
using System.Collections.Generic;

namespace Wayfinder.Canvas {

public class Submission {

public int id {get; set;}
public int assignment_id {get; set;}
public string url {get; set;}
public int user_id {get; set;}
public List<SubmissionComment> submission_comments {get; set;} 
public User user {get; set;} 
public string workflow_state {get; set;}
public string grade {get; set;}
public string body {get; set;}
public bool grade_matches_current_submission {get; set;}
public bool late {get; set;}
public int? attempt {get; set;}
public double? score {get; set;}
public bool? excused {get; set;}
public bool missing {get; set;}

}

public class SubmissionComment {
public int id {get; set;}
public string author_name {get; set;}
public string comment {get; set;}
public DateTime created_at {get; set;} 
public DateTime? edited_at {get; set;} 
}
}

public class UpdateGrades {
public int user_id {get; set;}
public string text_comment {get; set;}
}
