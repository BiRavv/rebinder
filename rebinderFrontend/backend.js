function BackendAction(address, type) {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `${address}@${type}`,
  });
}

window.addEventListener("DOMContentLoaded", () => {
  document
    .getElementById("add-scenario")
    .addEventListener("click", AddScenario);
  document
    .getElementById("add-scenario-done")
    .addEventListener("click", AddScenarioDone);
  document.getElementById("test-btn").addEventListener("click", one);
});

const scenarioHolder = document.getElementById("scenario-holder");
const scenarioPopup = document.getElementById("add-scenario-popup");

function AddScenario() {
  scenarioPopup.style.visibility = "visible";
}

const newScenarioName = document.getElementById("add-scenario-name");
const newScenarioWarning = document.getElementById("add-scenario-warning");

function one() {
  fetch("http://localhost:3102/", {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: `scenario@one`,
  });
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
    });
}
