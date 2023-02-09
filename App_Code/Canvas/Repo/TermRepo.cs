using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wayfinder.Canvas {

public class TermRepo {

private Client client;
private Cache<Term> TermCache;

public TermRepo(Client c)
	{
	this.client = c;

	TermCache = new Cache<Term>
			(
			new List<CachedAttribute>
				{
				new CachedAttribute { DBColumn="id", Attribute="id"},
				new CachedAttribute { DBColumn="name", Attribute="name"},
				new CachedAttribute { DBColumn="sis_term_id", Attribute="sis_term_id"},
				new CachedAttribute { DBColumn="start_at", Attribute="start_at"},
				new CachedAttribute { DBColumn="end_at", Attribute="end_at"}
				},
			"canvas_terms", "id"
			);
	}

public async Task<Term> GetCanvasTerm(int term_id)
	{
	// Special case: No real term
	if(term_id == 1 || term_id == 0)
		{
		return new Term {
			id=0,
			sis_term_id="",
			name="No Term",
			start_at= new DateTime(1981,2,28),
			end_at= new DateTime(2099,2,28)
			};
		}

	Term match = TermCache.RetrieveOne(Convert.ToString(term_id));

	if(match != null)
		return match;

	// Wow... there is no way to select a single term...
	string baseUrl = String.Format("{0}/accounts/1/terms", Constants.CanvasApiBase);

	HttpApi<TermListResult> api = new HttpApi<TermListResult>(client);
	List<Term> terms = (await api.GetAsync(baseUrl)).enrollment_terms;

	if(terms == null || terms.Count == 0)
		return null;

	foreach(Term t in terms)
		{
		if(t.id == term_id)
			{
			TermCache.Store(Convert.ToString(term_id), t);
			return t;
			}
		}
	
	return null;
	}

public async Task<List<Term>> GetCanvasTerms()
	{
	int page = 0;
	int pageSize = 100;
	var terms = new List<Term>();

	HttpApi<TermListResult> api = new HttpApi<TermListResult>(client);
	string baseUrl = String.Format("{0}/accounts/1/terms", Constants.CanvasApiBase);
	while(true)
		{
		string queryUrl = client.PageUrl(baseUrl, page++, pageSize);
		TermListResult result = (await api.GetAsync(queryUrl));

		terms.AddRange(result.enrollment_terms);

		if(result.enrollment_terms.Count < pageSize) break;
		}

	foreach(Term t in terms)
		{
		TermCache.Store(Convert.ToString(t.id), t);
		}

	return terms;
	}
}
}
