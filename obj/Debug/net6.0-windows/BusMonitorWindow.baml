   M S B A M L   `   `   `  ????B  >WpfApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:WpfApp1WpfApp1  5   	       WpfApp1.BusMonitorWindow   5   
   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:WpfApp1     5   	     ? Title$  BusMonitorWindow??5   	   $	??450??6"   $	??400??6/   
 ? Top$ 0??5	   	    ? Left$	 600??6   .??5      ? -   5         ?Name$ mainGrid??5       ? RowDefinitions 5   
   ?? 5      $??50 ?6   6*   ?? 5      $	??300 ?6   6+    ? ColumnDefinitions 5   
   ?? 5      $	??400 ?6   6-   .+?5   
   ?? $??0??6   $??0??6!   $??Stretch=?61   
???   ?$??50??6]   ?? 5   
   $??Bus Monitor??6   $??80??6'   $??0??62   $??0??6?   $??Right=?6O   $??Center=?6k   
???????6?   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ monitorDataGrid??5      $??2??6,   ??69   ?? 5      5      5      