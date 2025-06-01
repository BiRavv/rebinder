const { app, BrowserWindow, ipcMain, Tray, Menu } = require("electron");
const path = require("path");

let win;
let tray;

function createWindow() {
  win = new BrowserWindow({
    width: 700,
    height: 450,
    frame: false,
    resizable: false,
    transparent: true,
    autoHideMenuBar: true,
    hasShadow: false,
    icon: path.join(__dirname, "assets", "icon.png"), // <- ide kerül az ikon
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  win.loadFile("index.html"); // More robust than loadURL with file://

  win.webContents.openDevTools(); // Show DevTools on launch

  win.removeMenu();

  // Setup tray icon
  tray = new Tray(path.join(__dirname, "assets/icon.png"));
  tray.setToolTip("rebinder");

  const contextMenu = Menu.buildFromTemplate([
    {
      label: "Show",
      click: () => win.show(),
    },
    {
      label: "Quit",
      click: () => app.quit(),
    },
  ]);
  tray.setContextMenu(contextMenu);

  tray.on("click", () => {
    win.isVisible() ? win.hide() : win.show();
  });

  // IPC window controls
  ipcMain.on("window-close", () => win.close());
  ipcMain.on("window-minimize", () => win.minimize());
  ipcMain.on("window-hide", () => win.hide());
}

app.whenReady().then(createWindow);

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});

app.on("activate", () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow();
});
