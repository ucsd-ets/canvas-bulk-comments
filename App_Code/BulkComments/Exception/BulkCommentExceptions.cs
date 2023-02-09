using System;

namespace BulkComments {

public class NoIdentifierException: Exception { 
public NoIdentifierException(string message) : base(message) 
	{ 
	}
}
public class DuplicateIdentifierException: Exception { 
public DuplicateIdentifierException(string message) : base(message) 
	{ 
	}
}

public class DuplicateColumnException: Exception { 
public DuplicateColumnException(string message) : base(message) 
	{ 
	}
}

public class DuplicateMatchException: Exception { }
public class NoDataException: Exception { }
public class NoIdentifyingFieldException: Exception { }
public class NoCommentFieldException: Exception { }

}
