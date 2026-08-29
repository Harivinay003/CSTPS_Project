# CSTPS_Project
<img width="1920" height="1035" alt="Screenshot 2026-08-29 121234" src="https://github.com/user-attachments/assets/a07f691e-e083-4b27-8ef7-678960d83121" />
<img width="1920" height="1035" alt="Screenshot 2026-08-29 121301" src="https://github.com/user-attachments/assets/48e04ec7-0a85-4f00-8c07-9b87f55c9aaf" />
<img width="1920" height="1035" alt="Screenshot 2026-08-29 121254" src="https://github.com/user-attachments/assets/f1951e3d-1ec7-4051-88bc-432a9053b373" />
# Analytics Page – WPF SCADA / VirtualEMS

## Overview

The **Analytics Page** is a monitoring and analysis module of the WPF SCADA / VirtualEMS application. It is designed to display historical and calculated data from connected field devices and help operators analyze parameters such as:

* Energy consumption
* Run hours
* Alarms
* Trips
* Device performance
* Historical values
* Date-wise data
* Shift / Day / Week / Month based analytics

The Analytics Page works together with the application's backend APIs and SQL databases to retrieve historical data and present it through interactive charts and information cards.

The main objective is to provide operators with a simple way to select a field device, select a date or duration, and view the corresponding historical information.

---

# 1. Main Features

The Analytics Page provides the following major features:

### 1.1 Field Device Selection

The page displays available field devices in a side navigation/list.

Example:

```text
Field Devices
 ├── CONV_101A
 ├── CONV_102A
 ├── CR01
 ├── WF01
 └── CONV_120
```

The user can select a field device to view its analytics.

When the page loads, the first available field device is selected automatically.

---

### 1.2 Date Selection

The Analytics Page contains a date selector.

The selected date is used when requesting historical analytics data.

For example:

```text
Selected Date: 2026-08-29
```

Changing the date should refresh the analytics data and chart.

This allows the operator to compare device behavior across different dates.

---

### 1.3 Duration Selection

The page supports different analytics durations.

Typical duration options include:

```text
Shift
Day
Week
Month
```

The selected duration determines the time range used for retrieving and displaying analytics data.

For example:

```text
Date: 29-Aug-2026
Duration: Day
```

will display the data associated with that day.

---

# 2. Analytics Cards

The Analytics Page contains summary cards for important device information.

Typical cards include:

```text
Energy
Alarm
Trips
Run Hours
```

These cards provide a quick overview before the user examines the detailed chart.

---

## 2.1 Energy

The Energy card displays the calculated energy consumption of the selected device.

Depending on the device configuration, the value may be calculated from energy-related tags or database values.

Example:

```text
ENERGY
125.40 kWh
```

Energy data can be displayed for the selected:

* Date
* Field Device
* Duration

---

## 2.2 Alarms

The Alarm card provides information about alarms associated with the selected device.

The analytics system can be used to determine:

* Number of alarms
* Alarm occurrences
* Historical alarm information
* Date/time of alarm events

Example:

```text
ALARMS
12
```

---

## 2.3 Trips

The Trips card provides information about device trips.

A trip generally represents a protection/interlock event that caused the equipment to stop or enter a fault state.

Example:

```text
TRIPS
3
```

---

## 2.4 Run Hours

The Run Hours card displays the operating time of the selected equipment.

Run-hour information can be obtained from configured run-hour tags.

For example:

```text
RUN HOURS
1042.5 Hrs
```

The backend can retrieve run-hour information from the appropriate database/tag configuration.

---

# 3. Architecture

The Analytics functionality follows a frontend/backend/data-layer architecture.

```text
┌─────────────────────────────┐
│       WPF Analytics UI      │
│                             │
│  Device Selection           │
│  Date Selection             │
│  Duration Selection         │
│  Analytics Cards            │
│  Charts                     │
└──────────────┬──────────────┘
               │
               │ API Request
               ▼
┌─────────────────────────────┐
│       Backend API            │
│                             │
│ Live / Analytics Endpoints  │
│ Calculation Logic           │
│ Data Processing             │
└──────────────┬──────────────┘
               │
               │ SQL Queries
               ▼
┌─────────────────────────────┐
│        SQL Database          │
│                             │
│ Live Data                    │
│ Analog Data                  │
│ Device Configuration         │
│ Calculations                 │
│ Run Hours                    │
│ Historical Data              │
└─────────────────────────────┘
```

---

# 4. Frontend

The Analytics frontend is responsible for:

1. Displaying field devices.
2. Handling field-device selection.
3. Handling date selection.
4. Handling duration selection.
5. Displaying analytics cards.
6. Requesting data from the backend.
7. Updating charts.
8. Refreshing displayed values when the selection changes.

The frontend should not directly access the SQL database.

Instead, it communicates with the backend through APIs.

---

# 5. Backend

The backend is responsible for:

* Retrieving configuration.
* Retrieving historical values.
* Reading calculation definitions.
* Querying SQL Server.
* Processing raw data.
* Calculating analytics.
* Returning structured data to the frontend.

The backend contains APIs used by the Analytics Page and other SCADA pages.

---


# 10. Run Hours

Run hours are an important part of the Analytics Page.

The application retrieves run-hour information using configured run-hour tags.

Example tags discussed during development:

```text
CR01_Run_Hrs_FWD
CR01_Run_Hrs_REV
```

These represent forward and reverse running hours for a compressor.

Example configuration:

```text
CR01_Run_Hrs_FWD
Tag Type: RUN_HRS
Address: 1042

CR01_Run_Hrs_REV
Tag Type: RUN_HRS
Address: 1044
```

The exact tag addresses and names depend on the device configuration.

---

# 11. Run Hours Calculation

When a device has multiple run-hour tags, the backend must identify the correct tags before calculating the final run-hour value.

For example:

```text
Forward Run Hours
        +
Reverse Run Hours
        =
Total Run Hours
```

Depending on the device configuration, the calculation may instead require selecting the appropriate run-hour value.

The Analytics Page should therefore use tag configuration rather than assuming a fixed tag name for every device.

---

# 12. Historical Data

Historical analytics are retrieved from SQL Server.

The system may use tables containing historical analog or live values.

For example, the application uses historical analog data for calculations such as run hours.

The database can contain records similar to:

```text
Time
TagId
Value
Device
```

The backend filters the records based on:

* Field device
* Tag
* Selected date
* Duration
* Required calculation

---

# 13. Date-Based Analytics

The selected date is an important parameter in the analytics request.

Example:

```text
Selected Date = 2026-08-29
```

The backend should retrieve data corresponding to that date rather than always returning the latest available data.

This is important because analytics are historical.

For example:

```text
29-Aug-2026
Energy = 125 kWh
Alarms = 4
Trips = 1
```

and:

```text
28-Aug-2026
Energy = 143 kWh
Alarms = 7
Trips = 2
```

should return different results.

---

# 14. Field Device Selection

The Analytics Page maintains a list of available field devices.

When a device is selected:

```text
SelectedFieldDevice
        ↓
Identify device configuration
        ↓
Identify required tags
        ↓
Request historical data
        ↓
Calculate analytics
        ↓
Update cards
        ↓
Update chart
```

This prevents the chart from displaying data belonging to another device.

---

# 15. Default Device Selection

When the Analytics Page initially loads, the first field device should be selected automatically.

Conceptually:

```csharp
if (FieldDevices.Any())
{
    SelectedFieldDevice = FieldDevices.First();
}
```

This ensures that the user does not see an empty analytics section when the page opens.

---

# 16. Chart

The Analytics Page uses a chart to visualize historical data.

The chart is updated based on the selected:

* Field Device
* Date
* Duration
* Parameter

The chart provides a visual representation of device behavior over time.

Example:

```text
Value
 ^
 |              *
 |          *       *
 |      *               *
 |   *
 | *
 +--------------------------------> Time
    08:00  10:00  12:00  14:00
```

---

# 17. Chart Data Flow

The chart follows this general flow:

```text
User selects device
        ↓
User selects date
        ↓
User selects duration
        ↓
Frontend sends request
        ↓
Backend receives parameters
        ↓
Backend queries SQL
        ↓
Backend processes data
        ↓
Backend returns JSON
        ↓
Frontend receives JSON
        ↓
Chart is updated
```

---

# 18. Plotly Chart

The analytics implementation uses Plotly for chart rendering.

A chart-loading function was implemented around the following concept:

```javascript
loadChart()
```

The function sends an AJAX request to the backend.

Example structure:

```javascript
$.ajax({
    url: '/EnergyConsumption?handler=EnergyData',
    type: 'GET',
    data: {
        selectedDate: selectedDate,
        fieldDevice: selectedFieldDevice,
        duration: duration
    },
    success: function (data) {
        // Update Plotly chart
    }
});
```

The exact handler and parameters should match the current backend implementation.

---

# 19. Energy Consumption Handler

The Energy Consumption analytics page uses an endpoint/handler similar to:

```text
/EnergyConsumption?handler=EnergyData
```

The handler receives the selected analytics parameters and returns the required data.

Typical parameters include:

```text
selectedDate
fieldDevice
duration
```

The returned data is then passed to Plotly.

---

# 20. AJAX Data Flow

The frontend can use AJAX to request analytics data without reloading the complete page.

Example flow:

```text
Date Picker
     │
     ▼
selectedDate
     │
     ├──────────────┐
     ▼              ▼
Field Device     Duration
     │              │
     └──────┬───────┘
            ▼
       AJAX Request
            │
            ▼
     EnergyData Handler
            │
            ▼
        SQL Query
            │
            ▼
      JSON Response
            │
            ▼
       Plotly Chart
```

---

# 21. Duration Logic

Duration determines the time range shown by the analytics chart.

## Shift

Used when the user wants to analyze a particular operating shift.

Example:

```text
Selected Date: 29-Aug-2026
Duration: Shift
```

The backend determines the appropriate shift time range based on the configured shift logic.

---

## Day

Displays data for the selected date.

Example:

```text
29-Aug-2026 00:00
        ↓
29-Aug-2026 23:59
```

---

## Week

Displays analytics for the week containing the selected date.

Example:

```text
Monday → Sunday
```

The exact week calculation should be controlled by the backend so that all analytics use the same calendar logic.

---

## Month

Displays analytics for the month containing the selected date.

Example:

```text
August 2026
```

The backend should determine the first and last date of the selected month.

---

# 22. SQL Database Interaction

The backend communicates with SQL Server using the configured connection string.

Example:

```csharp
var connStr =
    config.GetConnectionString("ConfigDBConnString");
```

The application validates the connection string before attempting to connect.

Example:

```csharp
if (string.IsNullOrWhiteSpace(connStr))
{
    return Results.Problem(
        "Configuration DB connection string not found.");
}
```

A SQL connection is then created:

```csharp
using var con = new SqlConnection(connStr);
await con.OpenAsync();
```

---

# 23. Database Separation

The application may use multiple databases depending on the type of data.

Typical separation:

```text
Configuration Database
        │
        ├── Device configuration
        ├── Tags
        ├── Calculations
        └── Page configuration

Data Database
        │
        ├── Live values
        ├── Analog data
        ├── Historical values
        └── Runtime information
```

The exact database names depend on the deployed VirtualEMS configuration.

---

# 24. Dynamic Database Selection

Some analytics functions use dynamically determined database names.

For example, historical data may be stored according to a monthly database structure.

Conceptually:

```text
DataDB_2026_08
DataDB_2026_09
DataDB_2026_10
```

When the user selects a date, the backend determines which database contains the required historical records.

This is especially important for monthly historical data.

---

# 25. Data Freshness

Live data should not be treated as valid indefinitely.

A data freshness rule was considered for the live-data system:

```text
Current Time - Database Time > 1 minute
```

If the difference is greater than one minute, the value should be considered stale.

Instead of displaying an old value, the system should return:

```text
-
```

This prevents the UI from misleading the operator by showing old values as if they were current.

This rule primarily applies to live data. Historical analytics should continue to use their requested historical time range.

---

# 26. Error Handling

The Analytics Page should handle errors gracefully.

Possible errors include:

* Database connection failure
* Missing configuration
* Missing field device
* Missing tag
* Invalid date
* No historical data
* SQL query failure
* Invalid calculation configuration
* API/AJAX failure

The frontend should not crash when the backend returns an error.

Instead, it should show an appropriate state such as:

```text
No data available
```

or:

```text
Unable to load analytics data
```

---

# 27. No Data Handling

A valid request does not necessarily mean that data exists.

For example:

```text
Device: Compressor 01
Date: 01-Jan-2026
```

may have no historical records.

The chart should handle this gracefully.

Recommended behavior:

```text
No historical data available for the selected date.
```

The chart should not display stale data from the previous selection.

---

# 28. Selection Change Behavior

Whenever the user changes any of the following:

```text
Field Device
Date
Duration
```

the analytics should be refreshed.

Example:

```text
Device changed
      ↓
Reload analytics

Date changed
      ↓
Reload analytics

Duration changed
      ↓
Reload analytics
```

This ensures that the cards and chart always represent the current selection.

---

# 29. Preventing Incorrect Data

A major requirement of the Analytics Page is ensuring that data from one device/date does not appear under another selection.

For example, if the user changes:

```text
CONV_101A
```

to:

```text
CONV_102A
```

the previous Compressor 01 chart should not remain visible while Compressor 02 data is loading.

Recommended sequence:

```text
Selection changed
      ↓
Clear/mark current data as loading
      ↓
Request new data
      ↓
Receive response
      ↓
Update cards
      ↓
Update chart
```

---

# 30. Typical Component Structure

A possible project structure is:

```text
WPFSCADA/
│
├── Pages/
│   ├── Analytics.xaml
│   └── Analytics.xaml.cs
│
├── Controls/
│   ├── StatusCard.xaml
│   └── StatusCard.xaml.cs
│
├── Models/
│   ├── FieldDevice.cs
│   ├── AnalyticsData.cs
│   └── RunHours.cs
│
├── Services/
│   ├── ApiService.cs
│   └── AnalyticsService.cs
│
└── Assets/
    └── ...
```

For the VirtualEMS web/backend side:

```text
VirtualEMS/
│
├── Pages/
│   └── EnergyConsumption.cshtml
│
├── API/
│   └── LiveEndpoints.cs
│
├── Models/
│
├── Repositories/
│
└── Data/
```

The exact structure may differ depending on the current solution.

---

# 31. StatusCard

The application uses reusable status-card components for displaying analytics values.

For example:

```text
┌────────────────────────┐
│      ENERGY            │
│                        │
│      125.40 kWh        │
└────────────────────────┘
```

A reusable card allows the same UI component to be used for:

* Energy
* Alarms
* Trips
* Run Hours
* Temperature
* Other calculated parameters

---

# 32. Data Binding

WPF controls should use data binding where possible.

Example:

```xml
<TextBlock
    Text="{Binding EnergyValue}" />
```

For a selected field device:

```xml
<ListBox
    ItemsSource="{Binding FieldDevices}"
    SelectedItem="{Binding SelectedFieldDevice}" />
```

For the selected date:

```xml
<DatePicker
    SelectedDate="{Binding SelectedDate}" />
```

This keeps the UI synchronized with the underlying data.

---

# 33. Property Change Notification

If the Analytics Page uses `INotifyPropertyChanged`, changes to selected values can trigger data refresh logic.

Example:

```csharp
public event PropertyChangedEventHandler PropertyChanged;
```

When a property changes:

```csharp
PropertyChanged?.Invoke(
    this,
    new PropertyChangedEventArgs(nameof(SelectedDate)));
```

This is useful for keeping the analytics UI responsive to selection changes.

---

# 34. API Response

The backend should return a structured response.

A conceptual response can look like:

```json
{
    "fieldDevice": "Compressor 01",
    "date": "2026-08-29",
    "duration": "Day",
    "energy": 125.4,
    "runHours": 1042.5,
    "alarms": 4,
    "trips": 1,
    "chartData": [
        {
            "time": "08:00",
            "value": 10.5
        },
        {
            "time": "09:00",
            "value": 12.8
        },
        {
            "time": "10:00",
            "value": 14.1
        }
    ]
}
```

The actual response structure depends on the current implementation.

---

# 35. Analytics Request

A typical analytics request contains:

```text
Field Device
Selected Date
Duration
Parameter
```

Example:

```text
Field Device = CR01
Selected Date = 2026-08-29
Duration = Day
Parameter = Energy
```

The backend uses these values to construct the appropriate query.

---

# 36. Security and Validation

Backend APIs should validate incoming parameters.

Examples:

```text
Is the date valid?
Does the field device exist?
Does the requested tag exist?
Is the duration supported?
Is the database available?
```

Never directly concatenate untrusted values into SQL queries.

Prefer parameterized SQL commands.

---

# 37. Performance Considerations

Analytics queries can potentially process large amounts of historical data.

To maintain performance:

* Query only the required date range.
* Query only required tags.
* Avoid retrieving unnecessary columns.
* Use SQL indexes where appropriate.
* Aggregate data in SQL when practical.
* Avoid loading an entire historical table into memory.
* Cache configuration where appropriate.
* Avoid repeatedly querying static configuration.

---

# 38. Common Problems Encountered During Development

Several issues were encountered while developing the Analytics functionality.

### 38.1 Selected Date Not Affecting Chart

The chart initially could continue showing the previous/current data even after changing the date.

The expected behavior is:

```text
Date changed
    ↓
New request
    ↓
New SQL query
    ↓
New response
    ↓
Chart refresh
```

The selected date must be passed all the way from the UI to the backend query.

---

### 38.2 Field Device Not Applied

A similar issue can happen when the selected field device is not passed correctly.

The request must include the selected device.

```text
SelectedFieldDevice
        ↓
API parameter
        ↓
Backend
        ↓
Device configuration
        ↓
Correct tags
        ↓
Correct data
```

---

### 38.3 SQL Column Errors

Analytics development involved SQL errors such as:

```text
Invalid column name 'ResetTime'
```

When such an error occurs, the database schema and SQL query must be checked together.

The application should not assume a column exists simply because it is referenced in code.

---

### 38.4 Null Reference Errors

Null reference errors can occur when:

* No field device is selected.
* The field-device list is empty.
* A calculation is missing.
* A tag configuration is missing.
* API response data is null.

These conditions should be checked before accessing objects.

---

### 38.5 AJAX Errors

AJAX failures can happen because:

* Handler name is incorrect.
* URL is incorrect.
* Parameter names do not match.
* Backend returns an exception.
* JSON format is invalid.
* Database query fails.

The browser/network response should be checked when debugging.

---

# 39. Debugging Workflow

When analytics data is incorrect, follow this order:

```text
1. Check UI selection
       ↓
2. Check request parameters
       ↓
3. Check API/handler
       ↓
4. Check backend logs
       ↓
5. Check SQL query
       ↓
6. Check database records
       ↓
7. Check tag configuration
       ↓
8. Check calculation logic
       ↓
9. Check returned JSON
       ↓
10. Check chart binding/rendering
```

This prevents spending time debugging the chart when the actual problem is in the database or backend.

---

# 40. Verifying Database Data

Before debugging the frontend, verify that the expected historical data exists in SQL Server.

Check:

```text
Device
Tag
Date
Time
Value
```

For example:

```sql
SELECT *
FROM AnalogData
WHERE TagId = @TagId
ORDER BY Time DESC;
```

The actual query should use the project's database schema and configured table/column names.

---

# 41. Verifying Run-Hour Tags

When run hours are incorrect, verify:

```text
Tag Name
Tag Type
Tag Address
Device
Database
Historical values
```

For CR01 example:

```text
CR01_Run_Hrs_FWD
CR01_Run_Hrs_REV
```

Make sure the correct tags are mapped to the correct compressor.

---

# 42. Analytics Development Checklist

Before considering the Analytics Page complete, verify:

### UI

* [ ] Field devices are displayed.
* [ ] First field device is selected by default.
* [ ] Date picker works.
* [ ] Duration selector works.
* [ ] Analytics cards are displayed.
* [ ] Chart is displayed.
* [ ] Loading state is handled.
* [ ] No-data state is handled.
* [ ] Error state is handled.

### Backend

* [ ] Correct field device is received.
* [ ] Correct date is received.
* [ ] Correct duration is received.
* [ ] Correct tags are identified.
* [ ] Correct database is selected.
* [ ] SQL query is parameterized.
* [ ] Historical data is filtered correctly.
* [ ] Calculations are applied correctly.
* [ ] JSON response is valid.

### Database

* [ ] Device configuration exists.
* [ ] Tags exist.
* [ ] Calculation configuration exists.
* [ ] Historical records exist.
* [ ] Required columns exist.
* [ ] Run-hour records are available.
* [ ] Energy data is available.

### Chart

* [ ] Chart uses selected device.
* [ ] Chart uses selected date.
* [ ] Chart uses selected duration.
* [ ] Old data is cleared when selection changes.
* [ ] Empty results are handled.
* [ ] Chart updates after API response.

---

# 43. Current Development Status

The Analytics Page frontend and backend are being developed as part of the WPF SCADA / VirtualEMS system.

The overall workflow is:

```text
Field Device
     +
Selected Date
     +
Duration
     ↓
Analytics Request
     ↓
Backend
     ↓
Configuration
     +
Historical Database
     ↓
Calculations
     ↓
Analytics Result
     ↓
Cards + Chart
```

Some device-specific tag mappings and calculations may still need to be configured as the SCADA project is expanded.

---

# 44. Future Enhancements

Possible future improvements include:

### 44.1 Export

Allow users to export analytics data to:

```text
Excel
CSV
PDF
```

---

### 44.2 Advanced Filtering

Add filters for:

* Device
* Parameter
* Shift
* Date range
* Alarm type
* Trip type

---

### 44.3 Multiple Parameters

Allow users to select multiple parameters and compare them on the same chart.

Example:

```text
Temperature
Pressure
Energy
Run Hours
```

---

### 44.4 Comparison

Allow comparison between:

```text
Today vs Yesterday
This Week vs Previous Week
This Month vs Previous Month
```

---

### 44.5 Alarm Timeline

Add a dedicated timeline showing when alarms and trips occurred.

Example:

```text
08:15  Alarm
10:32  Trip
11:05  Reset
14:21  Alarm
```

---

### 44.6 Energy Trends

Provide:

```text
Hourly Energy
Daily Energy
Weekly Energy
Monthly Energy
```

with trend analysis.

---

# 45. Overall Data Flow

The complete Analytics architecture can be summarized as:

```text
                   ┌──────────────────┐
                   │     Operator     │
                   └────────┬─────────┘
                            │
                            ▼
                 ┌─────────────────────┐
                 │   Analytics Page    │
                 │                     │
                 │ Device              │
                 │ Date                │
                 │ Duration            │
                 └─────────┬───────────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │   API / Handler  │
                  └────────┬─────────┘
                           │
             ┌─────────────┼─────────────┐
             │             │             │
             ▼             ▼             ▼
       Configuration   Historical     Calculations
          Database        Data
             │             │             │
             └─────────────┼─────────────┘
                           ▼
                  ┌──────────────────┐
                  │ Analytics Result │
                  └────────┬─────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │ WPF / Web Presentation │
              │                        │
              │ Energy                 │
              │ Run Hours              │
              │ Alarms                 │
              │ Trips                  │
              │ Charts                 │
              └────────────────────────┘
```

---

# 46. Important Implementation Principle

The Analytics Page should always follow this principle:

> **The selected device, selected date, and selected duration must determine the data shown on the screen.**

The frontend should never assume that the currently displayed value is correct simply because it was retrieved successfully.

The complete data path must remain consistent:

```text
UI Selection
     ↓
Request Parameters
     ↓
Backend Processing
     ↓
Database Query
     ↓
Calculation
     ↓
API Response
     ↓
UI Update
```

Any mismatch in this chain can result in incorrect analytics.

---

# 47. Summary

The Analytics Page provides historical and calculated analytics for the SCADA system.

Its main responsibilities are:

* Device selection
* Date selection
* Duration selection
* Energy monitoring
* Run-hour monitoring
* Alarm analysis
* Trip analysis
* Historical data visualization
* Plotly chart rendering
* Backend API integration
* SQL Server integration
* Device/tag-based calculations

The architecture separates the presentation layer from the backend and database layers, allowing the system to support multiple devices and analytics calculations without tightly coupling the UI to the database.

The key workflow is:

```text
SELECT DEVICE
      ↓
SELECT DATE
      ↓
SELECT DURATION
      ↓
FETCH HISTORICAL DATA
      ↓
APPLY CALCULATIONS
      ↓
DISPLAY SUMMARY CARDS
      ↓
DISPLAY CHART
```

This structure allows the Analytics Page to become a reusable analytics framework for compressors, water chillers, IQF systems, cold stores, and other SCADA equipment.
