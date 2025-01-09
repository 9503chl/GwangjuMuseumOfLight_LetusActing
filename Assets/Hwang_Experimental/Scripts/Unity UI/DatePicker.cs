using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public class DatePicker : MonoBehaviour
    {
        public Button CurrentYearButton;
        public Text CurrentYearText;
        public Button PreviousYearButton;
        public Button NextYearButton;
        public Button CurrentMonthButton;
        public Text CurrentMonthText;
        public Button PreviousMonthButton;
        public Button NextMonthButton;
        public LayoutGroup WeekHeader;
        public LayoutGroup DayGrid;
        public Image CurrentCellImage;
        public Image SelectedCellImage;

        public Font CalendarFont = null;
        public Color NormalTextColor = Color.white;
        public Color SundayTextColor = new Color(1f, 0.25f, 0.25f, 1f);
        public Color SaturdayTextColor = new Color(0f, 0.5f, 1f, 1f);
        [Range(0f, 1f)]
        public float OutsideTextAlpha = 0.5f;

        private const string YEAR_TEXT_FORMAT = "yyyy";
        private const string MONTH_TEXT_FORMAT = "MMMM";
        private const string DAY_OF_WEEK_TEXT_FORMAT = "ddd";
        private const string DAY_TEXT_FORMAT = "d''";

        [SerializeField]
        private SystemLanguage calendarLanguage = SystemLanguage.Unknown;
        public SystemLanguage CalendarLanguage
        {
            get
            {
                return calendarLanguage;
            }
            set
            {
                if (calendarLanguage != value)
                {
                    calendarLanguage = value;
                    RebuildHeader();
                    RebuildCalendar();
                }
            }
        }

        [SerializeField]
        private DayOfWeek firstDayOfWeek = DayOfWeek.Sunday;
        public DayOfWeek FirstDayOfWeek
        {
            get
            {
                return firstDayOfWeek;
            }
            set
            {
                if (firstDayOfWeek != value)
                {
                    firstDayOfWeek = value;
                    RebuildHeader();
                    RebuildCalendar();
                }
            }
        }

        [SerializeField]
        private string yearTextFormat = YEAR_TEXT_FORMAT;
        public string YearTextFormat
        {
            get
            {
                return yearTextFormat;
            }
            set
            {
                if (yearTextFormat != value)
                {
                    yearTextFormat = value;
                    RebuildCalendar();
                }
            }
        }

        [SerializeField]
        private string monthTextFormat = MONTH_TEXT_FORMAT;
        public string MonthTextFormat
        {
            get
            {
                return monthTextFormat;
            }
            set
            {
                if (monthTextFormat != value)
                {
                    monthTextFormat = value;
                    RebuildCalendar();
                }
            }
        }

        [SerializeField]
        private string dayOfWeekTextFormat = DAY_OF_WEEK_TEXT_FORMAT;
        public string DayOfWeekTextFormat
        {
            get
            {
                return dayOfWeekTextFormat;
            }
            set
            {
                if (dayOfWeekTextFormat != value)
                {
                    dayOfWeekTextFormat = value;
                    RebuildHeader();
                }
            }
        }

        [SerializeField]
        private string dayTextFormat = DAY_TEXT_FORMAT;
        public string DayTextFormat
        {
            get
            {
                return dayTextFormat;
            }
            set
            {
                if (dayTextFormat != value)
                {
                    dayTextFormat = value;
                    RebuildCalendar();
                }
            }
        }

        [SerializeField]
        private bool interactable = true;
        public bool Interactable
        {
            get
            {
                return interactable;
            }
            set
            {
                if (interactable != value)
                {
                    interactable = value;
                    SetInteractable();
                }
            }
        }

        [SerializeField]
        private bool showOutsideCells = true;
        public bool ShowOutsideCells
        {
            get
            {
                return showOutsideCells;
            }
            set
            {
                if (showOutsideCells != value)
                {
                    showOutsideCells = value;
                    RebuildCalendar();
                }
            }
        }

        [SerializeField]
        private DateTime currentDate = DateTime.Today;
        public DateTime CurrentDate
        {
            get
            {
                return currentDate;
            }
            set
            {
                if (currentDate != value)
                {
                    currentDate = value;
                    RebuildCalendar();
                }
            }
        }

        [SerializeField]
        private DateTime selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get
            {
                return selectedDate;
            }
            set
            {
                if (selectedDate != value)
                {
                    selectedDate = value;
                    RebuildCalendar();
                }
            }
        }

        public UnityEvent onSelect = new UnityEvent();

        private class Cell
        {
            public Button CellButton;
            public Text CellText;
            public bool IsInside;
            public int CellValue;

            public static UnityAction<Button> OnButtonClick;

            public Cell(Button cellButton)
            {
                CellButton = cellButton;
                cellButton.onClick.RemoveAllListeners();
                cellButton.onClick.AddListener(CellButtonClick);
                CellText = cellButton.GetComponentInChildren<Text>();
            }

            private void CellButtonClick()
            {
                if (OnButtonClick != null)
                {
                    OnButtonClick(CellButton);
                }
            }
        }

        [NonSerialized]
        private List<Cell> cells = new List<Cell>();

        private void Start()
        {
            FillDayCells();
            RebuildHeader();
            RebuildCalendar();
        }

        private void OnEnable()
        {
            if (CurrentYearButton != null)
            {
                CurrentYearButton.interactable = interactable;
                CurrentYearButton.onClick.AddListener(CurrentYearButtonClick);
            }
            if (PreviousYearButton != null)
            {
                PreviousYearButton.interactable = interactable;
                PreviousYearButton.onClick.AddListener(PreviousYearButtonClick);
            }
            if (NextYearButton != null)
            {
                NextYearButton.interactable = interactable;
                NextYearButton.onClick.AddListener(NextYearButtonClick);
            }
            if (CurrentMonthButton != null)
            {
                CurrentMonthButton.interactable = interactable;
                CurrentMonthButton.onClick.AddListener(CurrentMonthButtonClick);
            }
            if (PreviousMonthButton != null)
            {
                PreviousMonthButton.interactable = interactable;
                PreviousMonthButton.onClick.AddListener(PreviousMonthButtonClick);
            }
            if (NextMonthButton != null)
            {
                NextMonthButton.interactable = interactable;
                NextMonthButton.onClick.AddListener(NextMonthButtonClick);
            }
            Cell.OnButtonClick += CellButtonClick;
            RebuildHeader();
            RebuildCalendar();
        }

        private void OnDisable()
        {
            if (CurrentYearButton != null)
            {
                CurrentYearButton.onClick.RemoveListener(CurrentYearButtonClick);
            }
            if (PreviousYearButton != null)
            {
                PreviousYearButton.onClick.RemoveListener(PreviousYearButtonClick);
            }
            if (NextYearButton != null)
            {
                NextYearButton.onClick.RemoveListener(NextYearButtonClick);
            }
            if (CurrentMonthButton != null)
            {
                CurrentMonthButton.onClick.RemoveListener(CurrentMonthButtonClick);
            }
            if (PreviousMonthButton != null)
            {
                PreviousMonthButton.onClick.RemoveListener(PreviousMonthButtonClick);
            }
            if (NextMonthButton != null)
            {
                NextMonthButton.onClick.RemoveListener(NextMonthButtonClick);
            }
            Cell.OnButtonClick -= CellButtonClick;
        }

        private void FillDayCells()
        {
            if (DayGrid != null)
            {
                cells.Clear();
                Button[] cellButtons = DayGrid.GetComponentsInChildren<Button>();
                foreach (Button cellButton in cellButtons)
                {
                    cells.Add(new Cell(cellButton));
                }
            }
        }

        private void SetInteractable()
        {
            if (CurrentYearButton != null)
            {
                CurrentYearButton.interactable = interactable;
            }
            if (PreviousYearButton != null)
            {
                PreviousYearButton.interactable = interactable;
            }
            if (NextYearButton != null)
            {
                NextYearButton.interactable = interactable;
            }
            if (CurrentMonthButton != null)
            {
                CurrentMonthButton.interactable = interactable;
            }
            if (PreviousMonthButton != null)
            {
                PreviousMonthButton.interactable = interactable;
            }
            if (NextMonthButton != null)
            {
                NextMonthButton.interactable = interactable;
            }
        }

        private string GetDateTimeText(DateTime dt, string format, string defFormat)
        {
            CultureInfo culture = calendarLanguage.GetCultureInfo();
            if (!string.IsNullOrEmpty(format))
            {
                try
                {
                    return dt.ToString(format, culture);
                }
                catch (Exception)
                {
                }
            }
            return dt.ToString(defFormat, culture);
        }

        private void SetCurrentCell(Cell cell)
        {
            if (CurrentCellImage != null)
            {
                if (Application.isPlaying && isActiveAndEnabled)
                {
                    if (cell != null)
                    {
                        CurrentCellImage.transform.SetParent(cell.CellButton.transform);
                        CurrentCellImage.transform.SetAsFirstSibling();
                        CurrentCellImage.rectTransform.anchoredPosition = Vector2.zero;
                        CurrentCellImage.enabled = true;
                    }
                    else
                    {
                        CurrentCellImage.transform.SetParent(transform);
                        CurrentCellImage.rectTransform.anchoredPosition = Vector2.zero;
                        CurrentCellImage.enabled = false;
                    }
                }
                else
                {
                    CurrentCellImage.enabled = false;
                }
            }
        }

        private void SetSelectedCell(Cell cell)
        {
            if (SelectedCellImage != null)
            {
                if (Application.isPlaying && isActiveAndEnabled)
                {
                    if (cell != null)
                    {
                        SelectedCellImage.transform.SetParent(cell.CellButton.transform);
                        SelectedCellImage.transform.SetAsLastSibling();
                        SelectedCellImage.rectTransform.anchoredPosition = Vector2.zero;
                        SelectedCellImage.enabled = true;
                    }
                    else
                    {
                        SelectedCellImage.transform.SetParent(transform);
                        SelectedCellImage.rectTransform.anchoredPosition = Vector2.zero;
                        SelectedCellImage.enabled = false;
                    }
                }
                else
                {
                    SelectedCellImage.enabled = false;
                }
            }
        }

        private void RebuildHeader()
        {
            if (WeekHeader != null)
            {
                DateTime todayDate = DateTime.Today;
                int days = (int)firstDayOfWeek - (int)todayDate.DayOfWeek;
                if (days < 0)
                {
                    days += 7;
                }    
                Text[] weekColumns = WeekHeader.GetComponentsInChildren<Text>();
                foreach (Text columnText in weekColumns)
                {
                    if (CalendarFont != null)
                    {
                        columnText.font = CalendarFont;
                    }
                    switch (todayDate.AddDays(days).DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            columnText.color = SundayTextColor;
                            break;
                        case DayOfWeek.Saturday:
                            columnText.color = SaturdayTextColor;
                            break;
                        default:
                            columnText.color = NormalTextColor;
                            break;
                    }
                    columnText.text = GetDateTimeText(todayDate.AddDays(days), dayOfWeekTextFormat, DAY_OF_WEEK_TEXT_FORMAT);
                    days++;
                }
            }
        }

        private void RebuildCalendar()
        {
            SetCurrentCell(null);
            SetSelectedCell(null);
            if (CurrentYearText != null)
            {
                if (CalendarFont != null)
                {
                    CurrentYearText.font = CalendarFont;
                }
                CurrentYearText.text = GetDateTimeText(currentDate, yearTextFormat, YEAR_TEXT_FORMAT);
                if (CurrentMonthText != null)
                {
                    if (CalendarFont != null)
                    {
                        CurrentMonthText.font = CalendarFont;
                    }
                    CurrentMonthText.text = GetDateTimeText(currentDate, monthTextFormat, MONTH_TEXT_FORMAT);
                }
            }
            else
            {
                if (CurrentMonthText != null)
                {
                    if (CalendarFont != null)
                    {
                        CurrentMonthText.font = CalendarFont;
                    }
                    CurrentMonthText.text = GetDateTimeText(currentDate, monthTextFormat, MONTH_TEXT_FORMAT);
                }
            }
            DateTime todayDate = DateTime.Today;
            DateTime firstDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            int firstDay = (int)firstDate.DayOfWeek - (int)firstDayOfWeek;
            if (firstDay < 0)
            {
                firstDay += 7;
            }
            int lastDay = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            int day = 1 - firstDay;
            Color cellDateColor;
            DateTime cellDate;
            foreach (Cell cell in cells)
            {
                cellDate = firstDate.AddDays(day - 1);
                switch (cellDate.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        cellDateColor = SundayTextColor;
                        break;
                    case DayOfWeek.Saturday:
                        cellDateColor = SaturdayTextColor;
                        break;
                    default:
                        cellDateColor = NormalTextColor;
                        break;
                }
                if (day <= 0)
                {
                    cell.IsInside = false;
                    cell.CellValue = day - 1;
                    cell.CellText.color = cellDateColor * OutsideTextAlpha;
                    cell.CellText.enabled = showOutsideCells;
                }
                else if (day > lastDay)
                {
                    cell.IsInside = false;
                    cell.CellValue = day - lastDay;
                    cell.CellText.color = cellDateColor * OutsideTextAlpha;
                    cell.CellText.enabled = showOutsideCells;
                }
                else
                {
                    cell.IsInside = true;
                    cell.CellValue = day;
                    cell.CellText.color = cellDateColor;
                    cell.CellText.enabled = true;
                    if (cellDate.Year == todayDate.Year && cellDate.Month == todayDate.Month && cellDate.Day == todayDate.Day)
                    {
                        SetCurrentCell(cell);
                    }
                    if (cellDate.Year == selectedDate.Year && cellDate.Month == selectedDate.Month && cellDate.Day == selectedDate.Day)
                    {
                        SetSelectedCell(cell);
                    }
                }
                if (CalendarFont != null)
                {
                    cell.CellText.font = CalendarFont;
                }
                cell.CellText.text = GetDateTimeText(cellDate, dayTextFormat, DAY_TEXT_FORMAT);
                cell.CellButton.enabled = cell.CellText.enabled;
                day++;
            }
        }

        private void CurrentYearButtonClick()
        {
            currentDate = new DateTime(DateTime.Today.Year, currentDate.Month, 1);
            RebuildCalendar();
        }

        private void PreviousYearButtonClick()
        {
            currentDate = currentDate.AddYears(-1);
            RebuildCalendar();
        }

        private void NextYearButtonClick()
        {
            currentDate = currentDate.AddYears(1);
            RebuildCalendar();
        }

        private void CurrentMonthButtonClick()
        {
            currentDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            RebuildCalendar();
        }

        private void PreviousMonthButtonClick()
        {
            currentDate = currentDate.AddMonths(-1);
            RebuildCalendar();
        }

        private void NextMonthButtonClick()
        {
            currentDate = currentDate.AddMonths(1);
            RebuildCalendar();
        }

        public void CellButtonClick(Button cellButton)
        {
            if (interactable)
            {
                if (cellButton != null)
                {
                    foreach (Cell cell in cells)
                    {
                        if (cell.CellButton == cellButton)
                        {
                            if (cell.IsInside)
                            {
                                currentDate = new DateTime(currentDate.Year, currentDate.Month, cell.CellValue);
                            }
                            else if (cell.CellValue < 0)
                            {
                                currentDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddDays(cell.CellValue);
                            }
                            else
                            {
                                currentDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)).AddDays(cell.CellValue);
                            }
                            selectedDate = currentDate;
                            RebuildCalendar();
                            onSelect.Invoke();
                            break;
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        private SystemLanguage validateLanguage = SystemLanguage.Unknown;

        private void OnValidate()
        {
            if (validateLanguage != calendarLanguage)
            {
                validateLanguage = calendarLanguage;
                firstDayOfWeek = calendarLanguage.GetCultureInfo().DateTimeFormat.FirstDayOfWeek;
            }
            FillDayCells();
            RebuildHeader();
            RebuildCalendar();
        }

        private void Reset()
        {
            if (CalendarFont == null)
            {
                CalendarFont = new Font("Arial");
            }
        }
#endif
    }
}
