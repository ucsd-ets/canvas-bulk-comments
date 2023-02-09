using System;
using System.Collections.Generic;

namespace Wayfinder.Canvas {

public class OauthTokenRepo {

public Cache<OauthToken> Cache;

public OauthTokenRepo()
	{
	Cache = new Cache<OauthToken>
		(
		new List<CachedAttribute>
			{
			new CachedAttribute { DBColumn="username", Attribute="username"},
			new CachedAttribute { DBColumn="canvas_user_id", Attribute="canvas_user_id"},
			new CachedAttribute { DBColumn="canvas_name", Attribute="canvas_name"},
			new CachedAttribute { DBColumn="token", Attribute="token"},
			new CachedAttribute { DBColumn="refresh_token", Attribute="refresh_token"},
			new CachedAttribute { DBColumn="expires", Attribute="expires"}
			},
		"oauth_cache", "username"
		);
	}

public OauthToken GetUsernameToken(string username)
	{
	OauthToken token = Cache.RetrieveOne(username);

	if(token == null)
		{
		OauthToken newToken = new OauthToken{ username=username };
		if(Cache.Store(username, newToken)) 
			token = newToken;
		}

	return token;
	}

public bool UpdateUsernameToken(OauthToken token)
	{
	return Cache.Store(token.username, token);
	}
}
}
