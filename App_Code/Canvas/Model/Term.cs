using System;
using System.Collections.Generic;

namespace Wayfinder.Canvas {

public class Term {

public int id {get; set;}
public string name {get; set;}
public string sis_term_id {get; set;}
public DateTime? start_at {get; set;}
public DateTime? end_at {get; set;}
}


public class TermListResult {
public List<Term> enrollment_terms {get; set;}
}

}
