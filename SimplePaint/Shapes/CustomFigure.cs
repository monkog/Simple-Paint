﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SimplePaint.Shapes
{
	/// <summary>
	/// Represents a figure composed of lines and ellipses
	/// </summary>
	public class CustomFigure
	{
		/// <summary>
		/// Gets the first node in the LinkedList.
		/// </summary>
		public LinkedListNode<CustomEllipse> FirstNode => FigureVertices.First;

		/// <summary>
		/// Gets the last node in the LinkedList.
		/// </summary>
		public LinkedListNode<CustomEllipse> LastNode => FigureVertices.Last;

		/// <summary>
		/// Gets the first vertex.
		/// </summary>
		public CustomEllipse FirstVertex => FirstNode.Value;

		/// <summary>
		/// Gets the last vertex.
		/// </summary>
		public CustomEllipse LastVertex => LastNode.Value;

		/// <summary>
		/// Gets or sets the color of the figure.
		/// </summary>
		public Color FigureColor { get; set; }

		/// <summary>
		/// Gets or sets the stroke thickness.
		/// </summary>
		public int StrokeThickness { get; set; }

		/// <summary>
		/// Gets or sets the maximum x coordinate of the figure.
		/// </summary>
		public int MaxX { get; set; }

		/// <summary>
		/// Gets or sets the maximum y coordinate of the figure.
		/// </summary>
		public int MaxY { get; set; }

		/// <summary>
		/// Gets or sets the minimum x coordinate of the figure.
		/// </summary>
		public int MinX { get; set; }

		/// <summary>
		/// Gets or sets the minimum y coordinate of the figure.
		/// </summary>
		public int MinY { get; set; }

		/// <summary>
		/// Gets or sets the vertex number.
		/// </summary>
		public int VertexNumber { get; set; }

		/// <summary>
		/// The figure shapes
		/// </summary>
		public LinkedList<IShape> FigureShapes { get; set; }
		/// <summary>
		/// Gets or sets the figure vertices.
		/// </summary>
		public LinkedList<CustomEllipse> FigureVertices { get; set; }

		/// <summary>
		/// Gets or sets the multi-sampling line.
		/// </summary>
		public CustomLine MultisamplingLine { get; set; }

		/// <summary>
		/// Gets or sets the color of the multi-sampling.
		/// </summary>
		public Color MultisamplingColor { get; set; }
		/// <summary>
		/// Gets the size of the vertex.
		/// </summary>
		public int VertexSize { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CustomFigure"/> class.
		/// </summary>
		/// <param name="point">The position point.</param>
		/// <param name="color">The color.</param>
		/// <param name="strokeThickness">The stroke thickness.</param>
		public CustomFigure(Point point, Color color, int strokeThickness)
		{
			FigureColor = color;
			StrokeThickness = strokeThickness;
			VertexSize = StrokeThickness + 6;
			FigureVertices = new LinkedList<CustomEllipse>();
			FigureVertices.AddFirst(new CustomEllipse(point));
			FigureShapes = new LinkedList<IShape>();
			FigureShapes.AddFirst(new CustomEllipse(point));
			MultisamplingLine = null;
			MultisamplingColor = Color.Azure;
			MaxX = point.X + 5;
			MinX = point.X - 5;
			MaxY = point.Y + 5;
			MinY = point.Y - 5;
			VertexNumber++;
		}

		/// <summary>
		/// Adds the vertex on the specified line.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		/// <param name="line">The line.</param>
		public void AddInbetweenVertex(CustomEllipse vertex, CustomLine line)
		{
			var previousNode = FigureShapes.Find(line).Previous;
			var nextNode = FigureShapes.Find(line).Next;

			var previousEllipse = previousNode.Value as CustomEllipse;
			CustomEllipse nextEllipse;

			if (nextNode == null)
				nextEllipse = FigureShapes.First.Value as CustomEllipse;
			else
				nextEllipse = nextNode.Value as CustomEllipse;

			if (((Math.Abs(vertex.Position.X - previousEllipse.Position.X) < 10 && Math.Abs(vertex.Position.Y - previousEllipse.Position.Y) < 10)
				|| (Math.Abs(vertex.Position.X - nextEllipse.Position.X) < 10) && Math.Abs(vertex.Position.Y - nextEllipse.Position.Y) < 10))
				return;

			FigureShapes.AddAfter(previousNode, new CustomLine(previousEllipse.Position, vertex.Position));
			FigureShapes.AddAfter(previousNode.Next, new CustomEllipse(vertex.Position));
			FigureShapes.AddAfter(previousNode.Next.Next, new CustomLine(vertex.Position, nextEllipse.Position));
			FigureShapes.Remove(line);

			FigureVertices.AddAfter(FigureVertices.Find(FindVertexWithValue(previousEllipse.Position)), vertex);

			VertexNumber++;

			if (vertex.Position.X + 5 > MaxX) MaxX = vertex.Position.X + 5;
			if (vertex.Position.X - 5 < MinX) MinX = vertex.Position.X - 5;
			if (vertex.Position.Y + 5 > MaxY) MaxY = vertex.Position.Y + 5;
			if (vertex.Position.Y - 5 < MinY) MinY = vertex.Position.Y - 5;
		}

		/// <summary>
		/// Adds the vertex to the newly constructed figure.
		/// </summary>
		/// <param name="point">The point.</param>
		public void AddVertex(Point point)
		{
			// We don't want to add next vertex too close to an existing one
			if (FigureVertices.Any(
				vertex => Math.Abs(vertex.Position.X - point.X) < 10 && Math.Abs(vertex.Position.Y - point.Y) < 10))
				return;

			FigureVertices.AddLast(new CustomEllipse(point));
			FigureShapes.AddLast(new CustomEllipse(point));
			var delta = VertexSize / 2;
			if (point.X + delta > MaxX) MaxX = point.X + delta;
			if (point.X - delta < MinX) MinX = point.X - delta;
			if (point.Y + delta > MaxY) MaxY = point.Y + delta;
			if (point.Y - delta < MinY) MinY = point.Y - delta;
			VertexNumber++;
		}

		/// <summary>
		/// Determines whether [is point in figure].
		/// </summary>
		/// <param name="point">The point.</param>
		/// <param name="figure">The figure.</param>
		/// <returns>True if the hit point is in a figure</returns>
		public static bool IsPointInFigure(Point point, CustomFigure figure)
		{
			if (point.X < figure.MinX - 5 || point.X > figure.MaxX + 5
				|| point.Y < figure.MinY - 5 || point.Y > figure.MaxY + 5)
				return false;

			var firstVertex = figure.FirstNode;
			var lastVertex = figure.LastNode;

			var isInFigure = false;

			for (var i = 0; i < figure.VertexNumber; i++)
			{
				var iX = firstVertex.Value.Position.X;
				var iY = firstVertex.Value.Position.Y;
				var jX = lastVertex.Value.Position.X;
				var jY = lastVertex.Value.Position.Y;

				if ((iY > point.Y) != (jY > point.Y)
					&& (point.X < (jX - iX) * (point.Y - iY) / (jY - iY) + iX))
					isInFigure = !isInFigure;
				lastVertex = firstVertex;
				firstVertex = firstVertex.Next;
			}
			return isInFigure;
		}

		/// <summary>
		/// Determines whether the specified point is vertex.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <param name="figure">The figure.</param>
		/// <param name="outVertex">The out vertex.</param>
		/// <returns>True if the hit point is vertex, false otherwise</returns>
		public static bool IsVertex(Point point, CustomFigure figure, out CustomEllipse outVertex)
		{
			outVertex = null;

			if (point.X < figure.MinX - 10 || point.X > figure.MaxX + 10
				|| point.Y < figure.MinY - 10 || point.Y > figure.MaxY + 10)
				return false;

			foreach (var vertex in figure.FigureVertices.Where(
				vertex => point.X < vertex.Position.X + 10 && point.X > vertex.Position.X - 10
					&& point.Y < vertex.Position.Y + 10 && point.Y > vertex.Position.Y - 10))
			{
				outVertex = vertex;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Finds the vertex with value.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns>First vertex with the specified position or null if one does not exist</returns>
		private CustomEllipse FindVertexWithValue(Point point)
		{
			return FigureVertices.FirstOrDefault(x => x.Position == point);
		}
	}
}