using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Device.Location;

namespace Lab3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<MapObject> obj = new List<MapObject>();
        List<PointLatLng> pts = new List<PointLatLng>();
        List<PointLatLng> nearestPointPosition = new List<PointLatLng>();
        List<MapObject> nearestObjects = new List<MapObject>();
        List<MapObject> Found = new List<MapObject>();
        Human human = null;
        Car car = null;
        public PointLatLng point { get; set; }
        public GMapMarker marker { get; private set; }
        static PointLatLng start;
        static PointLatLng end;
       
        public MainWindow()
        {
            
            InitializeComponent();
            Create.IsChecked = true;
          
        }
        private void MapLoaded(object sender, RoutedEventArgs e)
        {
            // настройка доступа к данным
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            // установка провайдера карт
            Map.MapProvider = GMapProviders.YandexMap;

            // установка зума карты
            Map.MinZoom = 2;
            Map.MaxZoom = 17;
            Map.Zoom = 15;
            // установка фокуса карты
            Map.Position = new PointLatLng(55.012823, 82.950359);

            // настройка взаимодействия с картой
            Map.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            Map.CanDragMap = true;
            Map.DragButton = MouseButton.Left;
        }
        private void area_ROUTE(object sender, MouseButtonEventArgs e)
        {

        }
         private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Create.IsChecked == true)
            {
               PointLatLng point = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);
                pts.Add(point);
                objType.Visibility = Visibility.Visible;
                objTitle.Visibility = Visibility.Visible;
                add1.Visibility = Visibility.Visible;
            }
            

        }


        private void Button_Click_2(object sender, RoutedEventArgs e) //добавить
        {
           
                MapObject mapObject = null;
            

            switch (objType.SelectedIndex)
            {
                case 0:
                    mapObject = new Car(objTitle.Text, Map.Position, Map);
                    car = new Car("Car", Map.Position, Map);
                    break;
                   
                case 1:
                    mapObject = new Human(objTitle.Text, Map.Position);
                    human = new Human("Human", Map.Position);
                    break;
                case 2:
                    mapObject = new Location(objTitle.Text, Map.Position);
                    break;
                case 3:
                    mapObject = new Area(objTitle.Text, pts);
                    break;
                    
                case 4:
                    mapObject = new Route(objTitle.Text, pts);
                    break;
                case 5:
                    human.moveTo(Map.Position);

                    marker = new GMapMarker(point)
                    {
                        Shape = new Image
                        {
                            Width = 32,
                            Height = 32,
                            ToolTip = "marker",
                            Margin = new System.Windows.Thickness(-16, -16, 0, 0),
                            Source = new BitmapImage(new Uri("pack://application:,,,/resources/Location/location.png"))
                        }
                    };
                    break;
            }

                obj.Add(mapObject);
                pts.Clear();
                ObjectList.Items.Clear();
                Found.Clear();

            foreach (MapObject cm in obj)
            {
                Map.Markers.Add(cm.getMarker());
                ObjectList.Items.Add(cm.getTitle());
                Found.Add(cm);
            }

            objTitle.Clear();
            
        }

    
        private void ObjectList_SelectionChanged(object sender, RoutedEventArgs e)

        {
            if (ObjectList.SelectedIndex >= 0)
            {
                int index = ObjectList.SelectedIndex;
                MapObject obj = Found[index];
                Map.Position = obj.getFocus();
            }
          
        }
       

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            objType.Visibility = Visibility.Visible;
            objTitle.Visibility = Visibility.Visible;
            add1.Visibility = Visibility.Visible;
            ObjectList.Visibility = Visibility.Visible;
            poisk.Visibility = Visibility.Hidden;
            kuda.Visibility = Visibility.Hidden;
            otkuda.Visibility = Visibility.Hidden;
            raast_do_obj.Visibility = Visibility.Hidden;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ObjectList.UnselectAll();
            ObjectList.Items.Clear();
            Found.Clear();
            
            foreach (MapObject mapObject in obj)
            {
                if (mapObject.getTitle().Contains(objTitle.Text))
                {
                    ObjectList.Items.Add(mapObject.getTitle());
                    Found.Add(mapObject);
                }
            }
        }

        private void Kuda_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                end = Map.Position;
            }
            catch
            {
                MessageBox.Show("Выберите конец маршрута!");
            }
        }

        private void Otkuda_Click(object sender, RoutedEventArgs e)
        {


            Map.Markers.Add(car.MoveTo(human.getPosition()));

            start = Map.Position;

            var besidedObj = obj.OrderBy(mapObject => mapObject.getDistance(start));

            Car nearestCar = null;
            // Human h = null;

            foreach (MapObject obj in obj)
            {
                if (obj.GetType().ToString() == "Lab3.Human" && obj.getFocus() == start)
                {
                    human = (Human)obj;
                    human.destination = end;
                    break;
                }
            }

            foreach (MapObject obj in besidedObj)
            {
                if (obj.GetType().ToString() == "Lab3.Car")
                {
                    nearestCar = (Car)obj;
                    break;
                }
            }

            nearestCar.MoveTo(start);
            nearestCar.Arrived += human.CarArrived;
            human.seated += nearestCar.passengerSeated;


        }

        private void Search_Checked(object sender, RoutedEventArgs e)
        {
            objType.Visibility = Visibility.Hidden;
            ObjectList.Visibility = Visibility.Visible;
            add1.Visibility = Visibility.Hidden;
            poisk.Visibility = Visibility.Visible;
            kuda.Visibility = Visibility.Visible;
            otkuda.Visibility = Visibility.Visible;
            raast_do_obj.Visibility = Visibility.Visible;


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)

        {


          
            if (Search.IsChecked == true)
            {

                ObjectList.Items.Clear();
                Found.Clear();

                obj.Sort((obj1, obj2) => obj1.getDistance(point).CompareTo(obj2.getDistance(point)));
                foreach (MapObject cm in obj)
                {
                    Map.Markers.Add(cm.getMarker());

                    ObjectList.Items.Add("Расстояние до " + cm.getTitle() + " - " + cm.getDistance(point).ToString("0.0") + " метров");

                    Found.Add(cm);

                }
            }
        }
    }
}
