using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace MarkerApp
{
    public partial class Form1 : Form
    {
        private GMapControl gMapControl;
        private GMapOverlay markersOverlay;
        private GMarkerGoogle draggedMarker;
        private bool isDragging;

        // Панель управления
        private Panel controlPanel;
        private TrackBar zoomTrackBar;

        // Полосы прокрутки
        private HScrollBar horizontalScrollBar;
        private VScrollBar verticalScrollBar;

        private DatabaseManager dbManager;

        public Form1()
        {
            InitializeComponent();
            InitializeGMap();
            InitializeControlPanel();
            InitializeScrollBars();
            dbManager = new DatabaseManager("Data Source=DESKTOP-L24K0F8\\SQLEXPRESS;Initial Catalog=MarkersDB;Integrated Security=True");
            LoadMarkersFromDatabase();
        }

        private void InitializeGMap()
        {
            gMapControl = new GMapControl();
            gMapControl.Dock = DockStyle.Fill;
            gMapControl.MapProvider = GMapProviders.GoogleMap;
            gMapControl.Position = new PointLatLng(55.7558, 37.6176);
            gMapControl.MinZoom = 2;
            gMapControl.MaxZoom = 20;
            gMapControl.Zoom = 10;
            gMapControl.MouseDown += GMapControl_MouseDown;
            gMapControl.MouseMove += GMapControl_MouseMove;
            gMapControl.MouseUp += GMapControl_MouseUp;
            gMapControl.MouseClick += GMapControl_MouseClick;
            gMapControl.OnMapZoomChanged += GMapControl_OnMapZoomChanged;
            gMapControl.MouseWheel += GMapControl_MouseWheel;
            this.Controls.Add(gMapControl);

            markersOverlay = new GMapOverlay("markers");
            gMapControl.Overlays.Add(markersOverlay);
        }

        private void InitializeControlPanel()
        {
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 100;
            controlPanel.Padding = new Padding(5);
            controlPanel.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(controlPanel);

            zoomTrackBar = new TrackBar();
            zoomTrackBar.Minimum = 2;
            zoomTrackBar.Maximum = 20;
            zoomTrackBar.Value = (int)gMapControl.Zoom;
            zoomTrackBar.TickStyle = TickStyle.Both;
            zoomTrackBar.Dock = DockStyle.Left;
            zoomTrackBar.Width = 200;
            zoomTrackBar.ValueChanged += (sender, e) => { gMapControl.Zoom = zoomTrackBar.Value; };
            controlPanel.Controls.Add(zoomTrackBar);

            Button resetButton = new Button();
            resetButton.Text = "Сбросить";
            resetButton.Width = 100;
            resetButton.Click += (sender, e) => { gMapControl.Zoom = 10; };
            controlPanel.Controls.Add(resetButton);
        }

        private void InitializeScrollBars()
        {
            horizontalScrollBar = new HScrollBar();
            horizontalScrollBar.Dock = DockStyle.Bottom;
            horizontalScrollBar.Minimum = -100;
            horizontalScrollBar.Maximum = 100;
            horizontalScrollBar.ValueChanged += HorizontalScrollBar_ValueChanged;
            this.Controls.Add(horizontalScrollBar);

            verticalScrollBar = new VScrollBar();
            verticalScrollBar.Dock = DockStyle.Right;
            verticalScrollBar.Minimum = -100;
            verticalScrollBar.Maximum = 100;
            verticalScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;
            this.Controls.Add(verticalScrollBar);
        }

        private void HorizontalScrollBar_ValueChanged(object sender, EventArgs e)
        {
            // Изменение долготы (движение вправо или влево)
            double offsetX = horizontalScrollBar.Value / 100.0;
            gMapControl.Position = new PointLatLng(gMapControl.Position.Lat, gMapControl.Position.Lng + offsetX);
        }

        private void VerticalScrollBar_ValueChanged(object sender, EventArgs e)
        {
            // Изменение широты (движение вверх или вниз)
            double offsetY = verticalScrollBar.Value / 100.0;
            gMapControl.Position = new PointLatLng(gMapControl.Position.Lat - offsetY, gMapControl.Position.Lng); // Меняем знак на минус
        }

        private void GMapControl_OnMapZoomChanged()
        {
            zoomTrackBar.Value = (int)gMapControl.Zoom;
        }

        private void GMapControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && gMapControl.Zoom < gMapControl.MaxZoom)
            {
                gMapControl.Zoom++;
            }
            else if (e.Delta < 0 && gMapControl.Zoom > gMapControl.MinZoom)
            {
                gMapControl.Zoom--;
            }
        }

        private void LoadMarkersFromDatabase()
        {
            try
            {
                using (SqlDataReader reader = dbManager.GetMarkers())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        double latitude = reader.GetDouble(2);
                        double longitude = reader.GetDouble(3);

                        GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(latitude, longitude), GMarkerGoogleType.red_dot)
                        {
                            Tag = id,
                            ToolTipText = name
                        };
                        markersOverlay.Markers.Add(marker);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке меток: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GMapControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                foreach (GMapMarker marker in markersOverlay.Markers)
                {
                    if (marker.IsMouseOver)
                    {
                        draggedMarker = marker as GMarkerGoogle;
                        isDragging = true;
                        break;
                    }
                }
            }
        }

        private void GMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedMarker != null)
            {
                PointLatLng newPosition = gMapControl.FromLocalToLatLng(e.X, e.Y);
                draggedMarker.Position = newPosition;
            }
        }

        private void GMapControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedMarker != null)
            {
                isDragging = false;
                SaveMarkerPosition((int)draggedMarker.Tag, draggedMarker.Position.Lat, draggedMarker.Position.Lng);
                draggedMarker = null;
            }
        }

        private void GMapControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PointLatLng point = gMapControl.FromLocalToLatLng(e.X, e.Y);
                AddMarkerToDatabase(point.Lat, point.Lng);
            }
        }

        private void AddMarkerToDatabase(double latitude, double longitude)
        {
            try
            {
                dbManager.AddMarkerToDatabase("Новая метка", latitude, longitude);
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(latitude, longitude), GMarkerGoogleType.red_dot)
                {
                    ToolTipText = "Новая метка"
                };
                markersOverlay.Markers.Add(marker);
                MessageBox.Show("Метка успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении метки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMarkerPosition(int id, double latitude, double longitude)
        {
            try
            {
                dbManager.UpdateMarkerPosition(id, latitude, longitude);
                MessageBox.Show("Позиция метки успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении позиции метки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
