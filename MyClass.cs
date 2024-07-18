/*
 * Создано в SharpDevelop.
 * Пользователь: PKorobkin
 * Дата: 14.10.2022
 * Время: 16:05
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HostMgd.Windows;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Teigha.Geometry;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using UT=PKNanoUserTools.Utilities.Utilities;
using PKNanoUserTools.Utilities;
using PKNanoUserTools.EditorInput;

using Platform = HostMgd;
using PlatformDb = Teigha;



namespace PKNanoUserTools
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class MyClass
	{
		[CommandMethod("PKSample",CommandFlags.UsePickSet)]
		public void Sample()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			var ed = doc.Editor;
			Database db = HostApplicationServices.WorkingDatabase;
			PlatformDb.DatabaseServices.TransactionManager tm = db.TransactionManager;
			
			var result = ed.GetSelection();
			if (result.Status!=PromptStatus.OK) return;
			
			var objects = result.Value.GetObjectIds();
			using (var tr = tm.StartTransaction())
			{
				var BlkTbl = tr.GetObject(db.BlockTableId,
				                          OpenMode.ForWrite) as BlockTable;

				var BlkTblRec = tr.GetObject(BlkTbl[BlockTableRecord.ModelSpace],
				                             OpenMode.ForWrite) as BlockTableRecord;
				
				foreach(ObjectId oid in objects)
				{
					var ent = tm.GetObject(oid,OpenMode.ForRead);
					
					var pt = ent as DBPoint;
					if(pt != null)
					{
						ed.WriteMessage("Point coords: X " + pt.Position.X + " : Y " + pt.Position.Y);
						DBPoint newPt = new DBPoint();
						newPt.Position=pt.Position;
						newPt.SetDatabaseDefaults(db);
						BlkTblRec.AppendEntity(newPt);
						tm.AddNewlyCreatedDBObject(newPt,true);
					}
				}
				
				var Ptresult = ed.GetPoint("тык");
				if(Ptresult.Status!=PromptStatus.OK)
				{
					tr.Commit();
					return;
				}
				ed.WriteMessage(Ptresult.Value.ToString());
				DBPoint newEdPt = new DBPoint();
				newEdPt.Position=Ptresult.Value;
				newEdPt.SetDatabaseDefaults(db);
				BlkTblRec.AppendEntity(newEdPt);
				tm.AddNewlyCreatedDBObject(newEdPt,true);
				tr.Commit();
			}
			var ds = Application.GetSystemVariable("DIMSCALE");
			ed.WriteMessage(ds.ToString());
			ed.WriteMessage("Success! WOW");
		}
		
		[CommandMethod("PKBreakLine",CommandFlags.UsePickSet)]
		public void BreakLine()
		{
			var bl = new BreakLine();
			bl.MakeBreakLine();
		}
		[CommandMethod("PKEditAttributes")]
		public void EditAttributes()
		{
			Utilities.AttributeEditor.EditAttributes();
		}

		[CommandMethod("PKEditAttributesSettings")]
		public void EditAttributesSettings()
		{
			Utilities.AttributeEditor.AttributeEditorSettingsSet();
		}
		
		[CommandMethod("PKListAttributes")]
		public void ListAttributes()
		{
			Utilities.AttributeEditor.ListAttributes();
		}
		[CommandMethod("PKMEditText", CommandFlags.UsePickSet)]
		public void MultiEditText()
		{
			MultiEditText ME = new MultiEditText();
			ME.Execute();
		}
		#region DimEdit
		DimEdit de;
		[CommandMethod("PKDimUnderline",CommandFlags.UsePickSet)]
		public void DimUnderline()
		{
			if(de==null) de=new DimEdit();
			de.UnderlineDim();
		}
		[CommandMethod("PKDimRewrite",CommandFlags.UsePickSet)]
		public void DimRewrite()
		{
			if(de==null) de=new DimEdit();
			de.RewriteDim();
		}
		
		[CommandMethod("PKDimSplit",CommandFlags.UsePickSet)]
		public void DimSplit()
		{
			if(de==null) de=new DimEdit();
			de.SplitDim();
		}
		
		[CommandMethod("PKDimMerge",CommandFlags.UsePickSet)]
		public void DimMerge()
		{
			if(de==null) de=new DimEdit();
			de.MergeDim();
		}
		
		[CommandMethod("PKDimTextWidth",CommandFlags.UsePickSet)]
		public void DimTextWidth()
		{
			if(de==null) de=new DimEdit();
			de.DimTextWidth();
		}
		#endregion
		
		/// <summary>
		/// Создание обозначения проема из двух отрезков
		/// </summary>
		[CommandMethod("PKHoleSign", CommandFlags.UsePickSet)]
		public void PkHoleSign()
		{
			HoleSign HS = new HoleSign();
			HS.Execute();
		}
		
		[CommandMethod("PKDimChain", CommandFlags.UsePickSet)]
		public void DimChain()
		{
			DimChain DC = new DimChain();
			DC.Execute();
		}
	}
	
	/// <summary>
	/// Замена команде BreakLine от ExpressTools. Надоело каждый раз вводить размеры для разных аннотативных масштабов.
	/// </summary>
	public class BreakLine
	{

		static Polyline DefaultPolyline()
		{
			Polyline result = new Polyline(0);
			/*Надо быть внимательным! если мы создаем объект полилинии с известным числом вершин, начинать добавлять точки надо с 1
			 * Если же мы просто в цикле добавляем новые точки, надо использовать метод AddVertexAt и начиная с нуля */
			result.AddVertexAt(0, new Point2d(-0.5, 0), 0, 0, 0);
			result.AddVertexAt(1, new Point2d(-0.25, -0.667), 0, 0, 0);
			result.AddVertexAt(2, new Point2d(0.25, 0.667), 0, 0, 0);
			result.AddVertexAt(3, new Point2d(0.5, 0), 0, 0, 0);
			return result;
		}

		//базовые точки и точки смещенные с краев
		Point3d p1, p2, p11, p21;

		//список точек для размещения брейклайнов
		List<Point2d> data;

		Line p1p2;
		Vector3d p1p2v;
		double scale = 1;

		//Размер значка и удлиннение линий
		double size = 3, overlength = 2.5;

		List<Entity> results = new List<Entity>();

		public void MakeBreakLine()
		{
			var ds = Application.GetSystemVariable("DIMSCALE");
			scale=(double)ds;

			size *= scale;
			overlength *= scale;
			

			var sset = Input.Implied();
			if (Input.StatusBad) //нет предварительно выбранных объектов. Используем старый механизм.
			{

				p1 = Input.Point("\nВведите первую точку"); if (Input.StatusBad) return;
				p2 = Input.Point("\nВведите вторую точку"); if (Input.StatusBad) return;
				

				p1p2 = new Line(p1, p2);
				p1p2v = p1p2.Delta.MultiplyBy(1 / p1p2.Length);

				Plane pl = new Plane(p1, p1p2.Delta.GetPerpendicularVector());

				p11 = p1.Add(p1p2v.MultiplyBy(overlength * -1));
				p21 = p2.Add(p1p2v.MultiplyBy(overlength));

				//заполняем точки пока ввод корректный. Если не введено ни одной точки, ставим единственную в середину
				data = new List<Point2d>();

				var pt = Input.Point("\nУкажите точку вставки символа или по умолчанию в середине");

				int cnt = 0;
				if (Input.StatusBad)
				{
					if (int.TryParse(Input.StringResult, out cnt) && cnt > 0)
					{
						data = Divide(cnt);
					}
					else
					{
						data.Add(UT.GetMiddle(p1, p2).to2d());
					}
				}

				while (Input.StatusOK)
				{
					data.Add(pt.OrthoProject(pl).to2d());
					pt = Input.Point("\nУкажите следующую точку вставки символа");
				}
				results.Add(Prepare());
			}
			else
			{
				using (var th = new TransactionHelper())
				{
					var ents = th.EditObjects(sset);
					var lines = ents.OfType<Line>();
					var polylines = ents.OfType<Polyline>();

					foreach(var l in lines)
					{
						th.WriteObject(MakeFromLine(l));
						l.Erase();
					}
					foreach (var pl in polylines)
					{
						th.WriteObject(MakeFromPolyLine(pl));
						pl.Erase();
					}
				}
			}
			using (var th = new TransactionHelper())
			{
				th.WriteObjects(results);
			}
			

		}
		/// <summary>
		/// нанизываем на линию символы исходя из точек
		/// </summary>
		/// <param name="data">массив точек для вставки символов</param>
		/// <returns></returns>
		Polyline Prepare()
		{
			var result = new Polyline();
			//сортируем точки исходя из расстояния от начала
			data.Sort((x, y) => x.GetDistanceTo(p1.to2d()).CompareTo(y.GetDistanceTo(p1.to2d())));
			result.AddVertexAt(0, p11.to2d(), 0, 0, 0);

			foreach (Point2d pt in data)
			{
				Matrix3d M = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
				                                            new Point3d(pt.X, pt.Y, 0), p1p2v, p1p2v.GetPerpendicularVector(), Vector3d.ZAxis);

				//переносим и масштабируем символ
				Polyline bl = DefaultPolyline();
				bl.TransformBy(M);
				M = Matrix3d.Scaling(size, new Point3d(pt.X, pt.Y, 0));
				bl.TransformBy(M);

				//добавляем символ по одной точке в результирующую полилинию в ее конец
				for (int i = 0; i < bl.NumberOfVertices; i++)
				{
					result.AddVertexAt(result.NumberOfVertices, bl.GetPoint2dAt(i), 0, 0, 0);
				}
			}

			result.AddVertexAt(result.NumberOfVertices, p21.to2d(), 0, 0, 0);
			result.SetDatabaseDefaults();
			return result;
		}
		/// <summary>
		/// Делим отрезок
		/// </summary>
		/// <param name="count">количество промежуточных точек</param>
		/// <returns></returns>
		List<Point2d> Divide(int count)
		{
			var result = new List<Point2d>();
			double increment = p1p2.Length / ((count + 1) / p1p2v.Length);
			for (int i = 1; i <= count; i++)
			{
				result.Add(p1.Add(p1p2v.MultiplyBy(increment * i)).to2d());
			}
			return result;
		}
		Polyline MakeFromLine(Line l)
		{
			var result = new Polyline();
			p1 = l.StartPoint; p2 = l.EndPoint;

			p1p2 = new Line(p1, p2);
			p1p2v = p1p2.Delta.MultiplyBy(1 / p1p2.Length);

			p11 = p1.Add(p1p2v.MultiplyBy(overlength * -1));
			p21 = p2.Add(p1p2v.MultiplyBy(overlength));


			result.AddVertexAt(0, p11.to2d(), 0, 0, 0);

			Point2d pt = new Point2d(p1.X * 0.5 + p2.X * 0.5, p1.Y * 0.5 + p2.Y * 0.5);
			Matrix3d M = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
			                                            new Point3d(pt.X, pt.Y, 0), p1p2v, p1p2v.GetPerpendicularVector(), Vector3d.ZAxis);

			//переносим и масштабируем символ
			Polyline bl = DefaultPolyline();
			bl.TransformBy(M);
			M = Matrix3d.Scaling(size, new Point3d(pt.X, pt.Y, 0));
			bl.TransformBy(M);

			//добавляем символ по одной точке в результирующую полилинию в ее конец
			for (int i = 0; i < bl.NumberOfVertices; i++)
			{
				result.AddVertexAt(result.NumberOfVertices, bl.GetPoint2dAt(i), 0, 0, 0);
			}


			result.AddVertexAt(result.NumberOfVertices, p21.to2d(), 0, 0, 0);
			result.SetDatabaseDefaults();
			return result;
		}
		Polyline MakeFromPolyLine(Polyline pl)
		{

			var result = new Polyline();

			int count = pl.NumberOfVertices - 1; //segments count
			count += pl.Closed ? 1 : 0;

			result.AddVertexAt(result.NumberOfVertices, pl.GetPoint2dAt(0), 0, 0, 0);

			for (int curr_vert = 0; curr_vert < count; curr_vert++)
			{
				if (pl.GetSegmentType(curr_vert) == SegmentType.Line)
				{
					p1 = pl.GetPoint3dAt(curr_vert); p2 = (pl.Closed && curr_vert == count - 1) ? pl.GetPoint3dAt(0) : pl.GetPoint3dAt(curr_vert + 1);
					p1p2 = new Line(p1, p2);
					p1p2v = p1p2.Delta.MultiplyBy(1 / p1p2.Length);

					Point2d pt = new Point2d(p1.X * 0.5 + p2.X * 0.5, p1.Y * 0.5 + p2.Y * 0.5);
					Matrix3d M = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
					                                            new Point3d(pt.X, pt.Y, 0), p1p2v, p1p2v.GetPerpendicularVector(), Vector3d.ZAxis);

					//переносим и масштабируем символ
					Polyline bl = DefaultPolyline();
					bl.TransformBy(M);
					M = Matrix3d.Scaling(size, new Point3d(pt.X, pt.Y, 0));
					bl.TransformBy(M);

					//добавляем символ по одной точке в результирующую полилинию в ее конец
					for (int i = 0; i < bl.NumberOfVertices; i++)
					{
						result.AddVertexAt(result.NumberOfVertices, bl.GetPoint2dAt(i), 0, 0, 0);
					}
					result.AddVertexAt(result.NumberOfVertices, pl.GetPoint2dAt((pl.Closed && curr_vert == count - 1) ? 0 : curr_vert + 1), 0, 0, 0);
				}
				else
				{
					result.AddVertexAt(result.NumberOfVertices, pl.GetPoint2dAt((pl.Closed && curr_vert == count - 1) ? 0 : curr_vert + 1), 0, 0, 0);
					result.SetBulgeAt(result.NumberOfVertices - 2, pl.GetBulgeAt(curr_vert));
				}
			}

			if (!pl.Closed)
			{
				//making offset of tails
				result.AddVertexAt(result.NumberOfVertices, pl.GetPoint2dAt(pl.NumberOfVertices-1), 0, 0, 0);

				p1 = pl.GetPoint3dAt(0); p2 = pl.GetPoint3dAt(1);
				p1p2 = new Line(p1, p2);
				p1p2v = p1p2.Delta.MultiplyBy(1 / p1p2.Length);
				p11 = p1.Add(p1p2v.MultiplyBy(overlength * -1));

				p1 = pl.GetPoint3dAt(pl.NumberOfVertices - 2); p2 = pl.GetPoint3dAt(pl.NumberOfVertices-1);
				p1p2 = new Line(p1, p2);
				p1p2v = p1p2.Delta.MultiplyBy(1 / p1p2.Length);
				p21 = p2.Add(p1p2v.MultiplyBy(overlength));

				result.SetPointAt(0, p11.to2d());
				result.SetPointAt(result.NumberOfVertices - 1, p21.to2d());
			}
			else result.Closed = true;
			result.SetDatabaseDefaults();
			return result;
		}
	}
	
}