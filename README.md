# What is ScanNet Downloader ?

ScanNet Downloader is a simple app to automate the downloading of scans and the creation of a .cbz archive.
You provide a scan url to download and the software will do the rest.

# What are the compatible scan websites ?

Currently, the compatible website are:
- www.scan-vf.net
- www.anime-sama.fr

> If the scan website you use is not part of this list, send me a message and I'll do my best to make ScanNet Downloader compatible with it.
> Some scan websites are well protected to prevent the user to download the scan image so some websites may never be compatible.

# How to use ScanNet Downloader ?

## Add the url of a scan

### What is the url I need to provide ?
In most of the case if you copy-paste the link of the first page of the scan you want to download it should work but depending on the website you may have more options.

#### Scan-vf.net

For scan-vf.net you can either provide directly the url of a chapter you want to download or the url the book and choose the chapters you want to download after.<br>
Here is an example of the 2 possibilities:
- You want to download a single chapter, provide the url of a chapter: https://www.scan-vf.net/one_piece/chapitre-1/1.
- You want to download several chapters, provide the url of the book: https://www.scan-vf.net/one_piece.

#### Anime-sama.fr

For anime-sama, open any chapter of the scan and copy-paste the url. You will choose the chapter later.<br>
The url should look like this: https://anime-sama.fr/catalogue/one-piece/scan/vf/.

### Where do I need to paste the scan url ?

In the app directory (next to the .exe file), you will find Settings.json. This files keep all the settings you can parameter when using the app.<br>
To add a the url of a scan look for the section called *ScansUrlAndCorrespondingChapters*. By default, it should look like this:
```
 "ScansUrlAndCorrespondingChapters":
 {
     "https://www.scan-vf.net/one_piece/chapitre-1/1": "",
     "https://anime-sama.fr/catalogue/one-piece/scan/vf/": "1-3;4;5"
 },
```
Every line inside *ScansUrlAndCorrespondingChapters* represent a scan url and, optionnally, the chapters to download. *Providing the chapter number here is optionnal, if no chapter are specified the app will ask you the chapters before downloading*. <br>

Let's check exactly what represent each line:
- `"https://www.scan-vf.net/one_piece/chapitre-1/1": ""`<br>
> The first part (before :) is the url of the scan (https://www.scan-vf.net/one_piece/chapitre-1/1), in this case the url already specify a chapter<br>
> The second part (after :) is the chapter to download (""), this is optionnal and since the chapter is already specified in the url we leave it empty.

- `"https://anime-sama.fr/catalogue/one-piece/scan/vf/": "1-3;4;5"`<br>
> The first part (before :) is the url of the scan (https://anime-sama.fr/catalogue/one-piece/scan/vf/)<br>
> The second part (after :) are the chapters to download ("1-3;4;5"), here we provide chapters 1 to 3 and chapter 4 and 5. [See more about Chapter selection here](#chapter-selection).

### Adding a new url

To add a new scan in the list you just new to add a new line. For example, if I want to add the first chapter of Berserk on anime-sama.fr, I will add `"https://anime-sama.fr/catalogue/berserk/scan/vf/": "1"`.<br>
Now the section *ScansUrlAndCorrespondingChapters* should look like this:

```
"ScansUrlAndCorrespondingChapters":
{
    "https://www.scan-vf.net/one_piece/chapitre-1/1": "",
    "https://anime-sama.fr/catalogue/one-piece/scan/vf/": "1-3;4;5",
    "https://anime-sama.fr/catalogue/berserk/scan/vf/": "1"
},
```

If I want to remove the default One-Piece urls, keep Berserk url and add the second chapter of Jujutsu Kaisen on scan-vf.net (`"https://www.scan-vf.net/jujutsu-kaisen": "2"`), I will have:

```
 "ScansUrlAndCorrespondingChapters":
 {
     "https://anime-sama.fr/catalogue/berserk/scan/vf/": "1",
     "https://www.scan-vf.net/jujutsu-kaisen": "2"
 },
```
> **! Every line inside *ScansUrlAndCorrespondingChapters* should end with a coma except the last one.**

### Chapter selection

The syntax to provide chapter is the same in Settings.json and inside the app if you're asked to provide a chapter.<br>
It's quite simple: either you provide a range of chapter (separated by -), either you provide single chapter and you can separate every entry with ;. Let's check some examples:
- I want chapters 1 to 10: `1-10`.
- I want chapter 2, 46 and 99: `2;46;99`.
- I want chapters 9 to 20 and chapter 61, : `9-20;61`.
- I want chapter 1, chapters 7 to 12, chapter 25 and chapters 30, : `1;7-12;25;30`.

# Available Settings
 
```
{
  "ScansUrlAndCorrespondingChapters":
  {
    "https://www.scan-vf.net/one_piece/chapitre-1/1": "",
    "https://anime-sama.fr/catalogue/berserk/scan/vf/": "1-3;4;5"
  },

  "CustomFolderPath": "",

  "CreateCbzArchive": true,

  "DeleteImagesAfterCbzCreation": false,

  "OpenOutputDirectoryWhenClosing": true,

  "ErrorsPauseApp": true,

  "AutoOpenJsonWhenNecessary": true
}
```
This is the whole default Settings.json, as you can see there is more settings than just *ScansUrlAndCorrespondingChapters*. Here is an explanation for each settings:

#### CustomFolderPath
By default, the download directory will be the path of windows download directory but you can change it to anything by providing a custom path here.<br>
> For example, here I set my download path to be "C:\Users\AnyUserName\ScanNetDownloader": <br>
> `"CustomFolderPath": "C:\\Users\\AnyUserName\\ScanNetDownloader",`<br>
> **All the \ in the path name must be doubled otherwise the path will not work correctly**

#### CreateCbzArchive
Do you to create a cbz archive once a chapter is downloaded, this can be set to **true** or **false**.

#### DeleteImagesAfterCbzCreation
When creating a cbz archive do you want to delete the image downloaded once the cbz is successfully created, this can be set to **true** or **false**.

#### OpenOutputDirectoryWhenClosing
Do you want to open the folder where everything has been downloaded when closing the app, this can be set to **true** or **false**.

#### ErrorsPauseApp
Do you to pause the app when an error happen or do you want to continue anyway, this can be set to **true** or **false**. If you select **false** a list of error will be displayed at the end of the download to let you know if some error happened during the download.

#### AutoOpenJsonWhenNecessary
Do you to automatically open Settings.json when the app require a change in it, this can be set to **true** or **false**.