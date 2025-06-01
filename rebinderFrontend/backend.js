const newScenarioName = document.getElementById("add-scenario-name");
const newScenarioWarning = document.getElementById("add-scenario-warning");
const scenarioHolder = document.getElementById("scenario-holder");
const scenarioPopup = document.getElementById("add-scenario-popup");
const currentScenarioName = document.getElementById("current-scenario-name");
const currentScenarioButton = document.getElementById("current-scenario-btn");

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
  if (newScenarioName.value.length < 3) {
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
      scenarioHolder.appendChild(sc);
      scenarioPopup.style.visibility = "collapse";
      newScenarioWarning.style.visibility = "hidden";
      newScenarioName.value = "";

      sc.addEventListener("click", () => selectScenario(sc));
    });
}

function selectScenario(scenario) {
  for (let i = 0; i < scenarioHolder.children.length; i++) {
    scenarioHolder.children[i].classList.remove("selected");
  }
  scenario.classList.add("selected");
  currentScenarioName.innerText = scenario.innerText;
}
