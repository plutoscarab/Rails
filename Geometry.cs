
// Geometry.cs

/*
 * This class provides some global methods for determine whether two line segments cross,
 * whether a line segment crosses a connected series of line segments (a polyline), or
 * whether two polylines cross. This is used during river generation to make sure rivers
 * don't intersect, and to determine which track segments intersect the rivers.
 * 
 * We need to use floats instead of ints for the polyline to make sure a river doesn't 
 * exactly fall on a milepost, otherwise it would require two bridges to cross the river
 * at that milepost, which is unjustified.
 * 
 */

using System;
using System.Drawing;

namespace Rails
{
	public sealed class Geometry
	{
		private Geometry()
		{
		}

		// a line segment intersects a polyline if it intersects any one of the line segments
		// contained in the polyline
		public static bool LineSegmentIntersectsPolyline(int x1, int y1, int x2, int y2, PointF[] points)
		{
			for (int i=0; i<points.Length-1; i++)
			{
				PointF p1 = points[i];
				PointF p2 = points[i+1];
				if (LineSegmentsIntersect(x1, y1, x2, y2, p1.X, p1.Y, p2.X, p2.Y))
					return true;
			}
			return false;
		}

		// determine if two line segments intersect
		//		ax,ay - bx,by = first line segment
		//		cx,cy - dx,dy = second line segment
		public static bool LineSegmentsIntersect(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
		{
			// If they're parallel, assume they can't intersect. Although we could
			// have co-linear line segments, it doesn't affect the game if we ignore them.
			float det = (bx-ax)*(cy-dy)-(by-ay)*(cx-dx);
			if (det == 0)
				return false;

			// find out where the extension of the second line segment crosses
			// the extension of the first line segment
			float t1 = (cx-ax)*(cy-dy)-(cy-ay)*(cx-dx);

			// determine if it crosses within the endpoints of the line sgement
			if (det > 0 && (t1 < 0 || t1 > det))
				return false;
			if (det < 0 && (t1 < det || t1 > 0))
				return false;

			// find out where the extension of the first line segment crosses
			// the extension of the second line segment
			float t2 = (bx-ax)*(cy-ay)-(by-ay)*(cx-ax);

			// determine if it crosses within the endpoints of the line segment
			if (det > 0 && (t2 < 0 || t2 > det))
				return false;
			if (det < 0 && (t2 < det || t2 > 0))
				return false;

			// Yes, they meet at one point.
			return true;
		}

		// Determine if two polylines intersect by checking each and every pair of
		// line segments. This is O(m*n) which is not the best algorithm for this,
		// but it's fast enough for our purposes and doesn't justify a more complicated
		// algorithm.
		public static bool PolylinesIntersect(PointF[] line1, PointF[] line2)
		{
			float bx = line1[0].X;
			float by = line1[0].Y;
			for (int i=0; i<line1.Length-1; i++)
			{
				float ax = bx;
				float ay = by;
				bx = line1[i+1].X;
				by = line1[i+1].Y;
				float dx = line2[0].X;
				float dy = line2[0].Y;
				for (int j=0; j<line2.Length-1; j++)
				{
					float cx = dx;
					float cy = dy;
					dx = line2[j+1].X;
					dy = line2[j+1].Y;
					if (LineSegmentsIntersect(ax, ay, bx, by, cx, cy, dx, dy))
						return true;
				}
			}
			return false;
		}
	}
}
