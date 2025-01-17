![Logo of the application]()

# What is ScanNet Downloader ?

ScanNet Downloader is a simple app to automate the downloading of scans and the creation of a .cbz archive.
You provide a scan url to download and the software will do the rest.

# What are the compatible scan websites ?

Currently, the compatible website are:
- www.scan-vf.net
- www.anime-sama.fr

> If the scan website you use is not part of this list, contact me or open an issue and I'll do my best to make ScanNet Downloader compatible with it.
> Some scan websites are well protected to prevent the user to download the scan image so some websites may never be compatible.

# How to use ScanNet Downloader ?

![Screenshot of the main window]()

## Add a new scan

![Screenshot of the main window without scans]()<br>
To add a new scan click on the "add new scan" button. A new window will open with a 3 steps process: entering the scan url, selecting the chapters, retrieving the scan data for the url and the chapters you specified.

### Enter the scan url

![Screenshot of the url input window]()<br>
Just enter the url of the scan in the text field and click "Next". If the url is invalid you will have an alert and you'll need to provide a valid url.

>If you encounter a case where you think the url is valid but an alert is triggered please open an issue and specify the url in it. That will help me to take into account possibilities I has not expected.

#### Compatibles url
In most of the case if you copy-paste the link of the first page of the scan you want to download it should work but depending on the website you may have more options.

##### Scan-vf.net

>You can either provide directly the url of a chapter or the url of the book.<br>
>Here is an example for both possibilities:
>- Url of a chapter: https://www.scan-vf.net/one_piece/chapitre-1/1.
>- Url of a book: https://www.scan-vf.net/one_piece.

##### Anime-sama.fr

>Open any chapter of the scan and copy-paste the url.<br>
>The url should look like this: https://anime-sama.fr/catalogue/one-piece/scan/vf/.

### Select the chapters

![Screenshot of the chapter selection window]()<br>
Add chapters one by one or directly enter a range of chapters. You can also mix both possibilities. If the url you entered was specifying a chapter the chapter should be added automatically to the chapter selection.

### Retrieve scan data
![Screenshot of the retrieve data window]()<br>
In this step the app will automatically look online to collect the data necessary to download the scan for each chapters. **The scans are not downloaded during this steps we only get the necessary data to successfully download the scans later**.
Wait for the completion of the scan data retrieval, once it's done you will be able to click the finish button and the scan data will be added in the scan manager view
![Screenshot of the scan manager window with some scan data]()<br>

## Download scans
![Screenshot of the download window]()<br>

### Select some scans to download
To download a scan you must first select them for download in the scan manager view. To select a scan just click on the checkbox to tick it. When you add a new scan, it is automatically selected for download by default.

### Download the scans
You can either directly click the "Start download" button in the scan manager view or open the download view and review the scans selected before starting the download from there. When you click to start the download a window will tell you where the scan will be download and ask you to confirm the start of the download. *If you want to change the download folder, cancel the download and change the download location in the options.*<br><br>
After a scan is downloaded, a .cbz archive is automatically created unless you disabled it in the option. Once the download is complete, by default, an explorer window is opened at the location of the downloaded files.
