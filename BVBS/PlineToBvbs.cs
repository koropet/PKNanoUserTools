/*
 * Создано в SharpDevelop.
 * Пользователь: Petr.Korobkin
 * Дата: 10.11.2026
 * Время: 11:34
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.Windows.Forms;
using App = HostMgd.ApplicationServices.Application;
using PKNanoUserTools.EditorInput;
using Platform = HostMgd;
using PlatformDb = Teigha;
using PKNanoUserTools.Utilities;

namespace PKNanoUserTools.BVBS
{
	/// <summary>
	/// Description of PlineToBvbs.
	/// </summary>
	public static class PlineToBvbs
	{
		public static void Execute()
		{
			var contents = new List<string>();
			var tags = new List<string>();
			var acDoc = App.DocumentManager.MdiActiveDocument;
			var acCurDb = acDoc.Database;
			var acEd = acDoc.Editor;
			
			string output="";
			
			var sset=Input.Objects("Выберите полилинии"); if(Input.StatusBad) return;
			using( var th=new TransactionHelper())
			{
				var plines=th.ReadObjects(sset).OfType<Polyline>();
				foreach(Polyline pl in plines)
				{
					var n = pl.NumberOfVertices;
					var points = new List<Point2d>();
					var bulges = new List<double>();
					var lengths = new List<double>();
					var angles = new List<double>();
					
					for(int i=0;i<n-1;i++)
					{
						var p0 = pl.GetPoint2dAt(i);
						var p1 = pl.GetPoint2dAt(i+1);
						var b = pl.GetBulgeAt(i);
						
						var bulgeAngle = Math.Atan(b)*2;
						
						
						var delta = p1-p0;
						var l = delta.Length;
						var a = delta.Angle;
						
						points.Add(p0);
						bulges.Add(bulgeAngle);
						lengths.Add(l);
						angles.Add(a);
							
						output+=string.Format("\n l= {0} a= {1} bulge = {2}",l,a,bulgeAngle);
					}
					if(pl.Closed)
					{
						var p0 = pl.GetPoint2dAt(n-1);
						var p1 = pl.GetPoint2dAt(0);
						var b = pl.GetBulgeAt(n-1);
						
						var delta = p1-p0;
						var l = delta.Length;
						var a = delta.Angle;
						
						var bulgeAngle = Math.Atan(b)*2;
						
						output+=string.Format("\n l= {0} a= {1} bulge = {2}",l,a,bulgeAngle);
						
						points.Add(p0);
						bulges.Add(bulgeAngle);
						lengths.Add(l);
						angles.Add(a);
						
						points.Add(p1);
					}
					else
					{
						points.Add(pl.GetPoint2dAt(n-1));
					}
					for(int i=1;i<bulges.Count;i++)
					{
						var a0=angles[i-1];
						var a1 = angles[i];
						var b0 = bulges[i-1];
						var b1 = bulges[i];
						
						var trueAngle = a1-a0-b1-b0;
						while(Math.Abs(trueAngle)>Math.PI)
						{
							if(trueAngle>Math.PI) trueAngle-=Math.PI*2;
							if(trueAngle<-Math.PI) trueAngle+=Math.PI*2;
						}
						
						output+=string.Format("\n true angle at {0} is {1}",i,trueAngle*180/Math.PI);
					}
				}
			}
			
			Console.WriteLine(output);
			
			Clipboard.SetText(output);
		}
	}
}
