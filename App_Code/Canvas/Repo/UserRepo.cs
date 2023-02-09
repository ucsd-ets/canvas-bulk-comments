using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wayfinder.Canvas {

public class UserRepo {

private Client client;
private Cache<User> UserCache;

public UserRepo(Client c)
	{
	this.client = c;

	UserCache = new Cache<User>
			(
			new List<CachedAttribute>
				{
				new CachedAttribute { DBColumn="canvas_userId", Attribute="id"}
				},
			"canvas_users", "ad_username"
			);
	}

public async Task<User> GetCanvasUser(string username, bool refresh = false)
	{
	// Canvas returns partial matches, so we must make sure to filter all but the exact match
	User match = null;
	
	if(!refresh)
		{
		match = UserCache.RetrieveOne(username);

		if(match != null)
			return match;
		}

	string baseUrl = String.Format("{0}/accounts/1/users?search_term={1}", Constants.CanvasApiBase, username);

	HttpApi<List<User>> api = new HttpApi<List<User>>(client);
	List<User> users = await api.GetAll<User>(baseUrl);

	if(users == null || users.Count == 0)
		return null;

	foreach(Canvas.User u in users)
		{
		if(u.login_id == username)
			{
			UserCache.Store(username, u);
			return u;
			}

		foreach(Canvas.Login l in await GetUserLogins(u))
			{
			if(l.unique_id == username)
				{
				UserCache.Store(username, u);
				return u;
				}
			}
		}
	
	return null;
	}

public async Task<List<Enrollments>> GetUserEnrollments(User user, string sis_course_id = null)
	{
	string search_term = "";
	if(sis_course_id != null)
		{
		search_term = String.Format("?sis_course_id[]={0}", sis_course_id);
		}

	string baseUrl = String.Format("{0}/users/{1}/enrollments{2}", Constants.CanvasApiBase, user.id, search_term);
	Console.WriteLine("Getting enrollments from {0} for user id {1} {2}", baseUrl, user.id, search_term);

	HttpApi<List<Enrollments>> api = new HttpApi<List<Enrollments>>(client);

	return await api.GetAll<Enrollments>(baseUrl);
	}
	
public async Task<List<Login>> GetUserLogins(User user)
	{
	string baseUrl = String.Format("{0}/users/{1}/logins", Constants.CanvasApiBase, user.id);
	//Console.WriteLine("Getting courses from {0} for user id {1}", baseUrl, user.id);

	HttpApi<List<Login>> api = new HttpApi<List<Login>>(client);

	return await api.GetAll<Login>(baseUrl);
	}

// return a list of section objects for a given course id.
public async Task<List<Canvas.Admin>> GetAccountAdmins(int accountID)
	{
	string baseUrl = String.Format("{0}/accounts/{1}/admins", Constants.CanvasApiBase, accountID);
	Console.WriteLine("Getting admins from {0} for account id {1}", baseUrl, accountID);

	HttpApi<List<Admin>> api = new HttpApi<List<Admin>>(client);
	return await api.GetAll<Admin>(baseUrl);
	}

public async Task<Canvas.Account> GetAccount(int accountID) {
	string baseUrl = String.Format("{0}/accounts/{1}", Constants.CanvasApiBase, accountID);
	Console.WriteLine("Getting account info from {0} for account id {1}", baseUrl, accountID);

	HttpApi<Account> api = new HttpApi<Account>(client);
	return await api.GetAsync(baseUrl);
}

public async Task<Canvas.Role> GetRoleInfo(int accountID, int roleID) {
	string baseUrl = String.Format("{0}/accounts/{1}/roles/{2}", Constants.CanvasApiBase, accountID, roleID);
	Console.WriteLine("Getting role info from {0} for account id {1} and role id {2}", baseUrl, accountID, roleID);

	HttpApi<Role> api = new HttpApi<Role>(client);
	return await api.GetAsync(baseUrl);
}

}
}
