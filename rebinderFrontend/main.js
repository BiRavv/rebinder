const { app, BrowserWindow, ipcMain, Tray, Menu } = require("electron");
const path = require("path");

let win;
let tray;

function createWindow() {
  win = new BrowserWindow({
    width: 700,
    height: 450,
    frame: false, // Remove title bar and window controls
    resizable: false, // Optional: prevent resizing
    transparent: true,
    autoHideMenuBar: true, // Hide menu bar
    hasShadow: false, // Optional: remove shadow
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
    },
  });

  win.loadURL(`file://${__dirname}/index.html`);

  win.removeMenu();
  win.setMenu(null);

  win.on("closed", () => {
    win = null;
  });

  tray = new Tray(path.join(__dirname, "assets/icon.png"));
  tray.setToolTip("rebinder");

  const contextMenu = Menu.buildFromTemplate([
    {
      label: "Show",
      click: () => {
        win.show();
      },
    },
    {
      label: "Quit",
      click: () => {
        app.quit();
      },
    },
  ]);

  tray.setContextMenu(contextMenu);

  tray.on("click", () => {
    win.isVisible() ? win.hide() : win.show();
  });

  ipcMain.on("window-close", () => {
    win.close();
  });

  ipcMain.on("window-minimize", () => {
    win.minimize();
  });

  ipcMain.on("window-hide", () => {
    win.hide(); // This hides the window (not quits)
  });

  tray.setImage(path.join(__dirname, "assets/icon.png"));
}

app.on("ready", createWindow);

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});

app.on("activate", () => {
  if (win === null) {
    createWindow();
  }
});
