using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Wayfinder;
using System.Web;

public class RepoFactory {
	private static Wayfinder.Canvas.Client _Client;
	private static Wayfinder.Canvas.Client Client 
		{
		get {
			if(_Client == null)
				{
				_Client = new Wayfinder.Canvas.Client();
				}

			return _Client;
			}
		}

	public static WayfinderTermRepo BuildRepo()
		{
		var repo = new Wayfinder.WayfinderTermRepo
			{
			CanvasCourseRepo = new Wayfinder.Canvas.CourseRepo(Client),
			CanvasTermRepo = new Wayfinder.Canvas.TermRepo(Client)
			};

		return repo;
		}

	public static Wayfinder.Canvas.UserRepo BuildUserRepo()
		{
		return new Wayfinder.Canvas.UserRepo(Client);
		}
	}
