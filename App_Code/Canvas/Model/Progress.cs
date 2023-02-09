using System;

namespace Wayfinder.Canvas {

public class Progress {
public int id {get; set;}
public int context_id {get; set;}
public string context_type {get; set;}
public int? user_id {get; set;}
public string tag {get; set;}
public int? completion {get; set;}
public string workflow_state {get; set;}
public DateTime? created_at {get; set;}
public DateTime? updated_at {get; set;}
public string message {get; set;}
}
}
