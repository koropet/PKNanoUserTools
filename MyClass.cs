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
			ed.WriteMessage("Success! WOW");
		}
	}
}