using System;

namespace Wayfinder.Canvas {

public class OauthToken {

public int oauth_id {get; set;}
public string username {get; set;}
public int canvas_user_id {get; set;}
public string canvas_name {get; set;}
public string token {get; set;}
public string refresh_token {get; set;}
public DateTime? expires {get; set;}

}

public class TokenResponse {
public string access_token {get; set;}
public string token_type {get; set;}
public TokenResponseUser user {get; set;}
public string refresh_token {get; set;}
public int expires_in {get; set;}
}

public class TokenResponseUser {
public int id {get;set;}
public string name {get; set;}
}

}
