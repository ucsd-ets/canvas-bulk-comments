using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wayfinder {

public class CachedAttribute {

public string DBColumn {get; set;}
public string Attribute {get; set;}

public string Parameter
	{
	get {
		return "@" + DBColumn;
		}
	}

}
}

