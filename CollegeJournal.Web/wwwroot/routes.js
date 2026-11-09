// Страница обращается к нашему API через fetch. SQL в браузере не выполняется.
let editingId = null;
let currentSearch = { type: 'all', value: '' };
const byId = id => document.getElementById(id);

function message(text, error = false) {
  byId('message').textContent = text;
  byId('message').className = error ? 'error' : 'success';
}

async function request(url, method = 'GET', data) {
  const response = await fetch(url, {
    method,
    headers: data === undefined ? {} : { 'Content-Type': 'application/json' },
    body: data === undefined ? undefined : JSON.stringify(data)
  });
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    throw new Error(body.message || body.title || `Ошибка запроса (${response.status}).`);
  }
  return response.status === 204 ? null : response.json();
}

// Текст из базы вставляем как textContent: он не должен превращаться в HTML.
function cell(row, value) {
  const td = document.createElement('td');
  td.textContent = value;
  row.append(td);
  return td;
}

function renderStudents(students) {
  const body = byId('student-rows');
  body.replaceChildren();
  byId('student-count').textContent = `Записей: ${students.length}`;
  if (students.length === 0) {
    const row = document.createElement('tr');
    cell(row, 'Студенты не найдены.').colSpan = 6;
    body.append(row);
  }
  for (const student of students) {
    const row = document.createElement('tr');
    for (const value of [student.id, student.lastName, student.firstName, student.age, student.groupName]) cell(row, value);
    const actions = cell(row, '');
    const edit = document.createElement('button');
    edit.textContent = 'Изменить';
    edit.onclick = () => editStudent(student);
    const remove = document.createElement('button');
    remove.textContent = 'Удалить';
    remove.onclick = () => run(async () => {
      if (!confirm(`Удалить студента ${student.lastName} ${student.firstName}?`)) return;
      await request(`/students/${student.id}`, 'DELETE');
      if (editingId === student.id) resetStudentForm();
      currentSearch = { type: 'all', value: '' };
      byId('search-name').value = '';
      byId('search-id').value = '';
      await refresh();
      message('Студент удалён.');
    }, remove);
    actions.append(edit, remove);
    body.append(row);
  }
}

async function loadStudents() {
  if (currentSearch.type === 'id') {
    renderStudents([]);
    renderStudents([await request(`/students/${encodeURIComponent(currentSearch.value)}`)]);
  } else {
    const query = encodeURIComponent(currentSearch.value);
    renderStudents(await request(`/students/all?name=${query}`));
  }
}

async function loadGroups() {
  const groups = await request('/groups/all');
  const selected = byId('group-id').value;
  const select = byId('group-id');
  select.replaceChildren(new Option('Выберите группу', ''));
  const body = byId('group-rows');
  body.replaceChildren();
  for (const group of groups) {
    select.add(new Option(group.name, group.id));
    const row = document.createElement('tr');
    for (const value of [group.id, group.name, group.studentCount]) cell(row, value);
    body.append(row);
  }
  select.value = selected;
}

async function refresh() {
  await loadGroups();
  await loadStudents();
}

function editStudent(student) {
  editingId = student.id;
  byId('first-name').value = student.firstName;
  byId('last-name').value = student.lastName;
  byId('age').value = student.age;
  byId('group-id').value = student.groupId;
  byId('form-title').textContent = `Изменить студента № ${student.id}`;
  byId('save-student').textContent = 'Сохранить';
  byId('cancel-edit').hidden = false;
  byId('last-name').focus();
}

function resetStudentForm() {
  editingId = null;
  byId('student-form').reset();
  byId('form-title').textContent = 'Добавить студента';
  byId('save-student').textContent = 'Добавить';
  byId('cancel-edit').hidden = true;
}

// Общая обёртка для сообщения об ошибке и блокировки повторного нажатия.
async function run(action, button) {
  if (button) button.disabled = true;
  message('');
  try { await action(); }
  catch (error) { message(error.message, true); }
  finally { if (button) button.disabled = false; }
}

byId('student-form').onsubmit = event => {
  event.preventDefault();
  run(async () => {
    const data = {
      firstName: byId('first-name').value.trim(), lastName: byId('last-name').value.trim(),
      age: Number(byId('age').value), groupId: Number(byId('group-id').value)
    };
    const updating = editingId !== null;
    await request(updating ? `/students/${editingId}` : '/students/add', updating ? 'PUT' : 'POST', data);
    resetStudentForm();
    currentSearch = { type: 'all', value: '' };
    byId('search-name').value = '';
    byId('search-id').value = '';
    await refresh();
    message(updating ? 'Изменения сохранены.' : 'Студент добавлен.');
  }, byId('save-student'));
};

byId('group-form').onsubmit = event => {
  event.preventDefault();
  run(async () => {
    await request('/groups/add', 'POST', { name: byId('group-name').value.trim() });
    byId('group-form').reset();
    await loadGroups();
    message('Группа добавлена. Теперь её можно выбрать в форме студента.');
  }, event.submitter);
};

byId('search-form').onsubmit = event => {
  event.preventDefault();
  currentSearch = { type: 'all', value: byId('search-name').value.trim() };
  run(loadStudents, event.submitter);
};
byId('id-form').onsubmit = event => {
  event.preventDefault();
  currentSearch = { type: 'id', value: byId('search-id').value };
  run(loadStudents, event.submitter);
};
byId('show-all').onclick = () => {
  currentSearch = { type: 'all', value: '' };
  byId('search-name').value = '';
  byId('search-id').value = '';
  run(loadStudents, byId('show-all'));
};
byId('cancel-edit').onclick = resetStudentForm;
run(refresh);
