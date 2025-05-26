const { app, BrowserWindow } = require('electron');
const path = require('path');

let win;

function createWindow() {
  win = new BrowserWindow({
    width: 600,
    height: 400,
    frame: false,             // Remove title bar and window controls
    resizable: false,         // Optional: prevent resizing
    transparent: true,
    autoHideMenuBar: true,    // Hide menu bar
    hasShadow: false,         // Optional: remove shadow
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
    },
  });

  win.loadURL(`file://${__dirname}/index.html`);

  win.on('closed', () => {
    win = null;
  });
}

app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (win === null) {
    createWindow();
  }
});
