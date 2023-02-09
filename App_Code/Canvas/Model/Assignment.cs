using System;

namespace Wayfinder.Canvas {

public class Assignment {

public int id {get; set;}
public bool published {get; set;}
public string name {get; set;}
public string description {get; set;}
public string html_url {get; set;}
public DateTime? due_at {get; set;}

public int course_id {get; set;}
}
}
