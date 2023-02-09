using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;

namespace Wayfinder.Canvas {

public class CourseRepo {

private Client client;

public CourseRepo(Client c)
	{
	this.client = c;
	}

public async Task<Course> GetCourse(int course_id, string optionalParams = "")
	{
	string baseUrl = String.Format("{0}/courses/{1}{2}", Constants.CanvasApiBase, course_id, optionalParams);

	HttpApi<Course> api = new HttpApi<Course>(client);
	return await api.GetAsync(baseUrl);
	}

public async Task<List<Course>> GetUserCourses(User user)
	{
	string baseUrl = String.Format("{0}/users/{1}/courses", Constants.CanvasApiBase, user.id);
	//Console.WriteLine("Getting courses from {0} for user id {1}", baseUrl, user.id);

	var courses = new List<Course>();

	HttpApi<List<Course>> api = new HttpApi<List<Course>>(client);
	return await api.GetAll<Course>(baseUrl);
	}

public async Task<List<Canvas.Course>> GetUserCourses(string username)
	{
	var ur = new Canvas.UserRepo(client);
	var u = await ur.GetCanvasUser(username);
	
	if(u == null) return new List<Course>(); 

	var CourseResponse = await GetUserCourses(u);

	if(CourseResponse == null)
		{
		// try updating the user
		u = await ur.GetCanvasUser(username, true);
		CourseResponse = await GetUserCourses(u);
		}

	if(CourseResponse == null)
		return new List<Canvas.Course>();

	return CourseResponse.Where(x=>!String.IsNullOrEmpty(x.name)).ToList();
	}

public async Task<List<Canvas.Enrollments>> GetCourseEnrollments(int courseID)
	{
	string baseUrl = String.Format("{0}/courses/{1}/enrollments", Constants.CanvasApiBase, courseID);
	Console.WriteLine("Getting enrollments from {0} for course id {1}", baseUrl, courseID);

	var enrollments = new List<Enrollments>();

	HttpApi<List<Enrollments>> api = new HttpApi<List<Enrollments>>(client);
	return await api.GetAll<Enrollments>(baseUrl);
	}

public async Task<List<Canvas.User>> GetCourseUsers(int courseID, string optionalParams = "")
	{
	string baseUrl = String.Format("{0}/courses/{1}/users{2}", Constants.CanvasApiBase, courseID, optionalParams);
	Console.WriteLine("Getting users from {0} for course id {1}", baseUrl, courseID);

	var users = new List<User>();

	HttpApi<List<User>> api = new HttpApi<List<User>>(client);
	return await api.GetAll<User>(baseUrl);
	}

///// SECTION METHODS
// return a list of section objects for a given course id.
public async Task<List<Canvas.Sections>> GetCourseSections(int courseID)
	{
	string baseUrl = String.Format("{0}/courses/{1}/sections", Constants.CanvasApiBase, courseID);
	Console.WriteLine("Getting sections from {0} for course id {1}", baseUrl, courseID);

	var sections = new List<Sections>();

	HttpApi<List<Sections>> api = new HttpApi<List<Sections>>(client);
	return await api.GetAll<Sections>(baseUrl);
	}

// creates a section from course id, name, and ISO8601 string of time.
public async Task<Sections> CreateCourseSection(int courseID, string name, string end_at, string sis_section_id)
	{
	string baseUrl = String.Format("{0}/courses/{1}/sections?course_section[name]={2}&course_section[end_at]={3}&course_section[sis_section_id]={4}&course_section[restrict_enrollments_to_section_dates]=true", Constants.CanvasApiBase, 
		courseID, name, end_at, sis_section_id);
	Console.WriteLine("Adding section at {0} in course {1} with name {2}", baseUrl, courseID, name);

	HttpApi<Sections> api = new HttpApi<Sections>(client);
	return await api.GetAsync(baseUrl, HttpMethod.Post, null);
	}

// assigns a student to a section
public async Task<Enrollments> AssignUserToSection(int userID, int sectionID, string enrollmentType) 
	{
	string baseUrl = String.Format("{0}/sections/{1}/enrollments?enrollment[user_id]={2}&enrollment[type]={3}&enrollment[enrollment_state]=active", Constants.CanvasApiBase, 
	sectionID, userID, enrollmentType);

	Console.WriteLine("Adding student {0} to section {1} at {2}", userID, sectionID, baseUrl);

	HttpApi<Enrollments> api = new HttpApi<Enrollments>(client);
	return await api.GetAsync(baseUrl, HttpMethod.Post, null);

	}

// a course ends by term if false, otherwise uses a set date.
public async Task<Object> UseCourseDate(int courseID, bool endDateEnabled) {

	int endDateEnabledInt = endDateEnabled ? 1 : 0;

	string baseUrl = String.Format("{0}/courses/{1}?course[restrict_enrollments_to_course_dates]={2}", Constants.CanvasApiBase, courseID, endDateEnabledInt);
	Console.WriteLine("Using end dates instead of terms in course {1} at {0} = {2}", baseUrl, courseID, endDateEnabled);

	HttpApi<Object> api = new HttpApi<Object>(client);
	return await api.GetAsync(baseUrl, HttpMethod.Put, null);
}

// change course from course id and ISO8601 string of time.
public async Task<Object> ChangeCourseEndDate(int courseID, string end_at)
	{
	string baseUrl = String.Format("{0}/courses/{1}?course[end_at]={2}", Constants.CanvasApiBase, courseID, end_at);
	Console.WriteLine("Changing end date of course {1} at {0}", baseUrl, courseID);

	HttpApi<Object> api = new HttpApi<Object>(client);
	return await api.GetAsync(baseUrl, HttpMethod.Put, null);
	}

public async Task<List<Canvas.Assignment>> GetCourseAssignments(int courseID)
	{
	string baseUrl = String.Format("{0}/courses/{1}/assignments", Constants.CanvasApiBase, courseID);
	Console.WriteLine("Getting assignments from {0} for course id {1}", baseUrl, courseID);

	var assignments = new List<Assignment>();

	HttpApi<List<Assignment>> api = new HttpApi<List<Assignment>>(client);
	return await api.GetAll<Assignment>(baseUrl);
	}

public async Task<Assignment> GetAssignment(int course_id, int assignment_id)
	{
	string baseUrl = String.Format("{0}/courses/{1}/assignments/{2}", Constants.CanvasApiBase, course_id, assignment_id);
	Console.WriteLine("Getting assignment {2} from {0} for course id {1}", baseUrl, course_id, assignment_id);

	HttpApi<Assignment> api = new HttpApi<Assignment>(client);
	return await api.GetAsync(baseUrl);
	}

public async Task<List<Submission>> GetAssignmentSubmissions(int assignment_id, int course_id)
	{
	string baseUrl = String.Format("{0}/courses/{1}/assignments/{2}/submissions?include[]=user&include[]=submission_comments", Constants.CanvasApiBase, course_id, assignment_id);
	Console.WriteLine("Getting submissions for assignment {2} from {0} for course id {1}", baseUrl, course_id, assignment_id);

	var submissions = new List<Submission>();

	HttpApi<List<Submission>> api = new HttpApi<List<Submission>>(client);
	return await api.GetAll<Submission>(baseUrl);
	}

public async Task<Object> AddCommentToSubmission(int assignment_id, int course_id, int user_id, string comment)
	{
	string baseUrl = String.Format("{0}/courses/{1}/assignments/{2}/submissions/{3}?comment[text_comment]={4}", Constants.CanvasApiBase, 
		course_id, assignment_id, user_id, HttpUtility.UrlEncode(comment));
	Console.WriteLine("Adding comment for user {3} on assignment {2} in course id {1}:\n * '{4}'", baseUrl, 
		course_id, assignment_id, user_id, HttpUtility.UrlEncode(comment));

	HttpApi<Object> api = new HttpApi<Object>(client);
	return await api.GetAsync(baseUrl, HttpMethod.Put, null);
	}

public async Task<Progress> AddCommentsToSubmission(int assignment_id, int course_id, List<UpdateGrades> updates)
	{
	string baseUrl = String.Format("{0}/courses/{1}/assignments/{2}/submissions/update_grades", 
		Constants.CanvasApiBase, course_id, assignment_id);

	var update_content = new List<KeyValuePair<string, string>>();
	foreach(var update in updates)
		{
		if(update.text_comment != null)
			{
			Console.WriteLine("Adding comment for user {3} on assignment {2} in course id {1}:\n * '{4}'", baseUrl, 
				course_id, assignment_id, update.user_id, update.text_comment);

			update_content.Add(new KeyValuePair<string, string>(
				String.Format("grade_data[{0}][text_comment]", update.user_id), 
				update.text_comment));
			}
		}
	
	var content = new FormUrlEncodedContent(update_content);
	Console.WriteLine("Updating comments: {0}", content.ReadAsStringAsync().Result);
	HttpApi<Progress> api = new HttpApi<Progress>(client);
	return await api.GetAsync(baseUrl, HttpMethod.Post, content);
	}

}
}
