using System;

namespace Wayfinder {
public class WayfinderTerm : IComparable{

public string trm_term_code {get; set;}
public string display_name {get; set;}
public DateTime? start_date {get; set;}

public bool floating_end_date
	{
	get {
		string trim = trm_term_code.Trim();
		return (String.IsNullOrEmpty(trim) || trim == "SAND" || trim == "BB_IMPORT" || trim == "PERM");
		}
	}
private DateTime? _end_date;
public DateTime? end_date
	{
	get {
		if(floating_end_date) 
			return DateTime.Now;
		return _end_date;
		}
	set {
		_end_date = value;
		}
	}
public string blackboard_term_pk1 {get; set;}
public int? canvas_term_id {get; set;}

public override int GetHashCode()
	{
	return trm_term_code.GetHashCode();
	}

public override bool Equals(Object other)
	{
	if(other == null) return false;
	if(!(other is WayfinderTerm)) return false;

	return ((WayfinderTerm) other).trm_term_code == trm_term_code;
	}

public int CompareTo(object other)
	{
	if (other == null) return 1;

	if(!(other is WayfinderTerm)) 
		throw new ArgumentException("Compared WayfinderTerm against null ");

	WayfinderTerm otherTerm = other as WayfinderTerm;

	return ((DateTime) otherTerm.end_date).CompareTo((DateTime) end_date);
    }

}
}
