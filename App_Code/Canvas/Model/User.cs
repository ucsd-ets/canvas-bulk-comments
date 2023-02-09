using System;

namespace Wayfinder.Canvas {

public class User {

public int id {get; set;}
public string name {get; set;}
public string sis_user_id {get; set;}
public string login_id {get; set;}
public string sortable_name { get; set; }
public string email { get; set; }

public string first_name
{
	get
	{
		string[] fullName = sortable_name.Split(new Char[]{','});
		if(fullName.Length == 2)
			return fullName[1].Trim();
		return fullName[0].Trim();
	}
}

public string last_name
{
	get
	{
		string[] fullName = sortable_name.Split(new Char[]{','});
		return fullName[0].Trim();
	}
}

}

}
