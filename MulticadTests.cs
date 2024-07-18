/*
 * Создано в SharpDevelop.
 * Пользователь: PKorobkin
 * Дата: 11.07.2024
 * Время: 8:56
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using Multicad.AplicationServices;
using Multicad.DataServices;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.Geometry;
using Multicad.DatabaseServices.StandardObjects;

namespace PKNanoUserTools
{
	/// <summary>
	/// Description of MulticadTests.
	/// </summary>
	public class MulticadTests
	{
		//выяснили, что именно из этого пространства надо помечать команды (возможно если первый раз команда была отсюда, следующие тоже идут отсюда)
		[Teigha.Runtime.CommandMethod("MAPI")]
		public void MAPI()
		{
			McContext.PopupNotification("Я тута");
			var doc = McDocument.ActiveDocument;
		}
	}
	
}
