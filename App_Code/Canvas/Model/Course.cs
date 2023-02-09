using System;

namespace Wayfinder.Canvas {

public class Course {

public int id {get; set;}
public string name {get; set;}
public string course_code {get; set;}
public string sis_course_id {get; set;}
public int account_id {get; set;}
public int enrollment_term_id {get; set;}
public DateTime? start_at {get; set;}
public DateTime? end_at {get; set;}
public string workflow_state {get; set;}
public bool restrict_enrollments_to_course_dates {get; set;}
public bool concluded {get; set;}
public Wayfinder.Canvas.Enrollments[] enrollments { get; set; }
public Wayfinder.Canvas.Sections[] sections {get; set; }
public bool hide_final_grades{get; set;}


public bool VisibleToEnrollments
	{
	get {
        if(workflow_state == "unpublished") 
			return false;
        if(restrict_enrollments_to_course_dates == false) 
			return true;

        DateTime now = DateTime.Now;

        if ((start_at == null || start_at < now) &&
            (end_at == null || end_at > now))
				return true;

        return false;
		}
	}

}

}
