/*
 * Создано в SharpDevelop.
 * Пользователь: PKorobkin
 * Дата: 14.07.2026
 * Время: 8:47
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

using System.Windows.Forms;

using Platform = HostMgd;
using PlatformDb = Teigha;

namespace PKNanoUserTools
{
	/// <summary>
	/// Description of LineSum.
	/// </summary>
	public class LineSum:UtilityClass
	{
		List<Entity> objects = new List<Entity>();
		static string delimiter= "\t"; //разделитель в списке длин
		
		public double GetLengthSum()
		{
			double sum = 0;
			var sset=Input.Objects("Выберите объекты для подсчета");
			if (Input.StatusBad) return 0;

			using (TransactionHelper th = new TransactionHelper())
			{
				objects = th.ReadObjects(sset);
				foreach (Entity ent in objects)
				{
					sum += UT.EntityLength(ent);
				}
			}
			return sum;
		}
		public void LengthList()
		{
			
			if(delimiter=="\n")
			{
				Tweet("Вертикальная группировка");
			}
			else if(delimiter=="\t")
			{
				Tweet("Горизонтальная группировка");
			}
			
			var list=new List<string>();
			var sset=Input.Objects("Выберите объекты",new [] {"Mode", "РЕжим"}, (s,e)=>
			                       {
			                       	if(delimiter=="\t")
			                       	{
			                       		delimiter="\n";
			                       		Tweet("Вертикальная группировка");
			                       	}
			                       	else if(delimiter=="\n")
			                       	{
			                       		delimiter="\t";
			                       		Tweet("Горизонтальная группировка");
			                       	}
			                       }
			                      ); if(Input.StatusBad) return;
			
			using(var th=new TransactionHelper())
			{
				objects=th.ReadObjects(sset);
				
				foreach(var o in objects)
				{
					//list.Add(String.Format("{0:F0}",UT.EntityLength(o)));
					list.Add(UT.EntityLength(o).ToString());
				}
			}
			
			Clipboard.SetText(list.Aggregate((s1,s2)=>s1+delimiter+s2));
		}
		public void CoordsList()
		{
			var list=new List<string>();
			var sset=Input.Objects("Выберите полилинии, отрезки или точки"); if(Input.StatusBad) return;
			
			using(var th=new TransactionHelper())
			{
				objects=th.ReadObjects(sset);
				
				foreach(var o in objects)
				{
					if(o is Polyline)
					{
						var pl_o = o as Polyline;
						for(int i=0; i<pl_o.NumberOfVertices;i++)
						{
							var pt = pl_o.GetPoint2dAt(i);
							list.Add(String.Format("{0:F3}\t{1:F3}",pt.X,pt.Y));
						}
					}
					if(o is Line)
					{
						var l_o = o as Line;
						var pt=l_o.StartPoint;
						list.Add(String.Format("{0:F3}/t{1:F3}",pt.X,pt.Y));
						pt=l_o.EndPoint;
						list.Add(String.Format("{0:F3}/t{1:F3}",pt.X,pt.Y));
					}
					if(o is DBPoint)
					{
						var pt = (o as DBPoint).Position;
						
						list.Add(String.Format("{0:F3}/t{1:F3}",pt.X,pt.Y));
					}
				}
			}
			
			Clipboard.SetText(list.Aggregate((s1,s2)=>s1+"\n"+s2));
		}
		
		public void AreaList()
		{
			
			if(delimiter=="\n")
			{
				Tweet("Вертикальная группировка");
			}
			else if(delimiter=="\t")
			{
				Tweet("Горизонтальная группировка");
			}
			
			var list=new List<string>();
			var sset=Input.Objects("Выберите полилинии или штриховки",new [] {"Mode", "РЕжим"}, (s,e)=>
			                       {
			                       	if(delimiter=="\t")
			                       	{
			                       		delimiter="\n";
			                       		Tweet("Вертикальная группировка");
			                       	}
			                       	else if(delimiter=="\n")
			                       	{
			                       		delimiter="\t";
			                       		Tweet("Горизонтальная группировка");
			                       	}
			                       }
			                      ); if(Input.StatusBad) return;
			
			using(var th=new TransactionHelper())
			{
				objects=th.ReadObjects(sset);
				
				foreach(var o in objects)
				{
					if(o is Polyline)
					{
						var pl_o = o as Polyline;
							list.Add(String.Format("{0:F3}",pl_o.Area));
					}
					if(o is Hatch)
					{
						var l_h = o as Hatch;
						list.Add(String.Format("{0:F3}",l_h.Area));
					}
				}
			}
			
			Clipboard.SetText(list.Aggregate((s1,s2)=>s1+delimiter+s2));
		}
	}
}
