const uri = "api/TodoItems";
let todos = [];

function getItems() {
    fetch(uri)
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => console.error("Unable to get items.", error));
}

function addItem() {
    const addNameTextbox = document.getElementById("add-name");

    const item = {
        isComplete: false,
        name: addNameTextbox.value.trim()
    };

    fetch(uri, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(item)
    })
        .then(response => response.json())
        .then(() => {
            getItems();
            addNameTextbox.value = "";
        })
        .catch(error => console.error("Unable to add item.", error));
}

function deleteItem(id) {
    fetch(`${uri}/${id}`, {
        method: "DELETE"
    })
        .then(() => getItems())
        .catch(error => console.error("Unable to delete item.", error));
}

function displayEditForm(id) {
    const item = todos.find(t => t.id === id);

    document.getElementById("edit-name").value = item.name;
    document.getElementById("edit-id").value = item.id;
    document.getElementById("edit-isComplete").checked = item.isComplete;
    document.getElementById("editForm").style.display = "block";
}

function updateItem() {
    const itemId = document.getElementById("edit-id").value;
    const item = {
        id: parseInt(itemId, 10),
        isComplete: document.getElementById("edit-isComplete").checked,
        name: document.getElementById("edit-name").value.trim()
    };

    fetch(`${uri}/${itemId}`, {
        method: "PUT",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(item)
    })
        .then(() => getItems())
        .catch(error => console.error("Unable to update item.", error));

    closeInput();

    return false;
}

function closeInput() {
    document.getElementById("editForm").style.display = "none";
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? "to-do" : "to-dos";

    document.getElementById("counter").innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
    const tBody = document.getElementById("todos");
    tBody.innerHTML = "";

    _displayCount(data.length);

    const button = document.createElement("button");

    data.forEach(item => {
        const isCompleteCheckbox = document.createElement("input");
        isCompleteCheckbox.type = "checkbox";
        isCompleteCheckbox.disabled = true;
        isCompleteCheckbox.checked = item.isComplete;

        const editButton = button.cloneNode(false);
        editButton.innerText = "Edit";
        editButton.setAttribute("onclick", `displayEditForm(${item.id})`);

        const deleteButton = button.cloneNode(false);
        deleteButton.innerText = "Delete";
        deleteButton.setAttribute("onclick", `deleteItem(${item.id})`);

        const tr = tBody.insertRow();

        const td1 = tr.insertCell(0);
        td1.appendChild(isCompleteCheckbox);

        const td2 = tr.insertCell(1);
        const textNode = document.createTextNode(item.name);
        td2.appendChild(textNode);

        const td3 = tr.insertCell(2);
        td3.appendChild(editButton);

        const td4 = tr.insertCell(3);
        td4.appendChild(deleteButton);
    });

    todos = data;
}
