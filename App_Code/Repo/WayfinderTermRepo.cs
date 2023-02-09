using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfinder {
public class WayfinderTermRepo {

public Canvas.CourseRepo CanvasCourseRepo { get; set; }
public Canvas.TermRepo CanvasTermRepo { get; set; }

private Cache<WayfinderTerm> TermCache;

public WayfinderTermRepo()
	{
	TermCache = new Cache<WayfinderTerm>
		(
		new List<CachedAttribute>
			{
			new CachedAttribute { DBColumn="trm_term_code", Attribute="trm_term_code"},
			new CachedAttribute { DBColumn="display_name", Attribute="display_name"},
			new CachedAttribute { DBColumn="start_date", Attribute="start_date"},
			new CachedAttribute { DBColumn="end_date", Attribute="end_date"},
			new CachedAttribute { DBColumn="blackboard_term_pk1", Attribute="blackboard_term_pk1"},
			new CachedAttribute { DBColumn="canvas_term_id", Attribute="canvas_term_id"}
			},
		"wayfinder_terms", "trm_term_code"
		);
	}

public WayfinderTerm CanvasWayfinderTerm(Canvas.Term canvas_term, bool force = false)
	{
	WayfinderTerm term = force ? null : TermCache.RetrieveOne(canvas_term.sis_term_id);

	if(term != null)
		{
		if(term.canvas_term_id != canvas_term.id)
			{
			term.canvas_term_id = canvas_term.id;
			TermCache.Store(canvas_term.sis_term_id, term);
			}
		return term;
		}

	term = new WayfinderTerm
		{
		trm_term_code = canvas_term.sis_term_id,
		display_name = canvas_term.name,
		canvas_term_id = canvas_term.id
		};
	
	if(canvas_term.start_at != null)
		term.start_date = (DateTime) canvas_term.start_at;
	else
		term.start_date = new DateTime(1980,12,27);

	if(canvas_term.end_at != null)
		term.end_date = (DateTime) canvas_term.end_at;
	else
		term.end_date = new DateTime(2099,2,28);
	
	TermCache.Store(canvas_term.sis_term_id, term);
	return term;
	}

}
}
