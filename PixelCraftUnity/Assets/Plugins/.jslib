mergeInto(LibraryManager.library, {
  DownloadFile: function (filenamePtr, byteArrayPtr, byteArrayLength) {
    var filename = UTF8ToString(filenamePtr);
    var byteArray = new Uint8Array(Module.HEAPU8.buffer, byteArrayPtr, byteArrayLength);
    var blob = new Blob([byteArray], { type: "image/png" });
    var link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
});