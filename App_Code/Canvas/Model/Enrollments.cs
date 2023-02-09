using System;

namespace Wayfinder.Canvas 
{
	public class Enrollments 
	{
		public int id {get; set;}
		public int user_id {get; set;}
		public int course_id {get; set;}
		public string type {get; set;}
		public string role { get; set; }

		public string sis_course_id {get; set;}
		public DateTime? updated_at {get; set;}
		public string sis_section_id {get; set;}
		public string sis_user_id {get; set;}

		public Wayfinder.Canvas.Grades grades {get; set;}
		public Wayfinder.Canvas.User user { get; set; }
	}
}
