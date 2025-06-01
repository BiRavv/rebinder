const newScenarioName = document.getElementById("add-scenario-name");
const newScenarioWarning = document.getElementById("add-scenario-warning");
const scenarioHolder = document.getElementById("scenario-holder");
const scenarioPopup = document.getElementById("add-scenario-popup");
const currentScenarioName = document.getElementById("current-scenario-name");
const currentScenarioButton = document.getElementById("current-scenario-btn");

let startingScenarios = [];
function getAllScenarios() {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `all_scenario@any`,
  })
    .then((res) => res.text())
    .then((response) => {
      console.log(response);
      response.split(";").forEach((name) => {
        if (name == "" || name == null) return;

        let sc = document.createElement("div");
        sc.classList.add("scenario");
        sc.innerText = name;
        sc.addEventListener("click", () => selectScenario(sc));
        scenarioHolder.appendChild(sc);

        if (scenarioHolder.firstChild != null) {
          scenarioHolder.firstChild.classList.add("selected");
        }
      });
    });
}

window.addEventListener("DOMContentLoaded", () => {
  document
    .getElementById("add-scenario")
    .addEventListener("click", AddScenario);
  document
    .getElementById("add-scenario-done")
    .addEventListener("click", AddScenarioDone);
  document
    .getElementById("current-scenario-btn")
    .addEventListener("click", StartCurrentScenario);
  getAllScenarios();
});

function AddScenario() {
  scenarioPopup.style.visibility = "visible";
}

function StartCurrentScenario() {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `scenario@${currentScenarioName.innerText}`,
  });

  const pauseSrc = new URL("assets/pause.svg", window.location.href).href;
  const startSrc = new URL("assets/start.svg", window.location.href).href;

  if (currentScenarioButton.src !== pauseSrc) {
    currentScenarioButton.src = pauseSrc;
  } else {
    currentScenarioButton.src = startSrc;
  }
}

function AddScenarioDone() {
  if (newScenarioName.value.length < 2 || newScenarioName.value.length > 7) {
    newScenarioWarning.style.visibility = "visible";
    return;
  }

  let finalName = newScenarioName.value;
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `add_scenario@${newScenarioName.value}`,
  })
    .then((res) => res.text())
    .then((response) => {
      finalName = response.split("@")[1];
      console.log(finalName);
    })
    .then(() => {
      let sc = document.createElement("div");
      sc.classList.add("scenario");
      sc.innerText = finalName;
      sc.addEventListener("click", () => selectScenario(sc));
      scenarioHolder.appendChild(sc);

      scenarioPopup.style.visibility = "collapse";
      newScenarioWarning.style.visibility = "hidden";
      newScenarioName.value = "";
    });
}

function selectScenario(scenario) {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `stop_scenario@${currentScenarioName.innerText}`,
  });

  for (let i = 0; i < scenarioHolder.children.length; i++) {
    scenarioHolder.children[i].classList.remove("selected");
  }
  scenario.classList.add("selected");
  currentScenarioName.innerText = scenario.innerText;

  const startSrc = new URL("assets/start.svg", window.location.href).href;
  currentScenarioButton.src = startSrc;
}
