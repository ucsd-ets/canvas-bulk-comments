# Canvas Bulk Comments Import Tool

## Caveats

This is not a true project: This was pieced together from two other projects, one of which is typically compiled into a DLL to be used by other projects, and a website with other tools and functions. That additional functionality is why many components are more complex than they might otherwise need to be.

If you wanted to use this, you will likely want to develop your own build process, using NuGet to install DLLs, following your own standards for code formatting, find and fix 'oops I meant to fix this' bad practices, etc.

## Layout

* Web UI / actions: /SubmissionComments
* Code directly used by it: /App\_Code/BulkComments
* Code used to authenticate LTI responses: /App_ode/LTI.cs
* Code used to interact with the API: /App_Code/Canvas, other top-level files

## Usage

* Create a site on an IIS server running ASP.Net 4.8
* Create an MSSQL database for the oauth key cache
* Create a developer key in Canvas
* Copy web.partial.template.config and populate the credentials
 * Connect to the above database
 * URL for Canvas, Canvas API
 * LTI Secret key that you'll use when creating a Canvas app
 * Oauth attributes (developer ID, credential)
   * Note that an admin API key is not required: This uses Oauth to interact with the permissions of the user, which helps limit risk.
* Set up an app that points to //SubmissionComments/Landing.aspx (you could probably quickly modify this so that 'public' privacy is not required).

## Oauth Key Cache table

    CREATE TABLE [dbo].[oauth_cache](
    	[oauth_id] [int] IDENTITY(1,1) NOT NULL,
    	[username] [nvarchar](255) NOT NULL,
    	[canvas_user_id] [int] NULL,
    	[canvas_name] [nvarchar](255) NULL,
    	[token] [nvarchar](255) NULL,
    	[refresh_token] [nvarchar](255) NULL,
    	[expires] [datetime] NULL,
    CONSTRAINT [PK_oauth_cache] PRIMARY KEY CLUSTERED ([oauth_id] ASC)
    )
