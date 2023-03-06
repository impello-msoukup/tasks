import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {
  title = 'tasks';
  tasks:any = []; // Update to model in next release
  get tName() { return this.tForm.get('tName'); }
  get tPriority() { return this.tForm.get('tPriority'); }
  get tStatus() { return this.tForm.get('tStatus'); }
  get f() { return this.tForm.controls; }
  tForm: FormGroup = this.fb.group({
    tName: new FormControl('', [Validators.required]),
    tPriority: new FormControl('', [Validators.required]),
    tStatus: new FormControl('', [Validators.required])
  }, { updateOn: 'submit' });
  invalidForm: boolean = false;
  invalidPriority: boolean = false;
  duplicateName: boolean = false;

  constructor(private fb: FormBuilder, private http: HttpClient) {

  }

  isTaskUnique(name: string): boolean {
    if(<Array<any>>this.tasks.find((task: any) => task.name == name)) {
      return false;
    }
    return true;
  }

  getTasks() {
    this.http.get("https://localhost:5001/v1/Tasks").subscribe((res) => {
      this.tasks = res;
    });
  }

  postTask() {
    let validForm: boolean = true;
    // Basic form validation
    if (!this.tForm.valid) {
      this.invalidForm = true;
      validForm = false;
    } else {
      this.invalidForm = false;
    }

    // Priority format validation
    if (isNaN(Number(this.f['tPriority'].value))) {
      this.invalidPriority = true;
      validForm = false;
    } else if (Number(this.f['tPriority'].value) >= 0 && Number(this.f['tPriority'].value) <= 100) {
      this.invalidPriority = false;
    } else {
      this.invalidPriority = true;
      validForm = false;
    }

    // Task name duplicity validation
    if(!this.isTaskUnique(this.f['tName'].value)) {
      this.duplicateName = true;
      validForm = false;
    } else {
      this.duplicateName = false;
    }

    // Exit in case of invalid inputs
    if (!validForm)
      return;

    // Post task on backend
    const body = { 'name': this.f['tName'].value, 'priority': Number(this.f['tPriority'].value), 'status': Number(this.f['tStatus'].value) };
    this.http.post("https://localhost:5001/v1/Tasks", body, { 'headers': { 'content-type': 'application/json' }}).subscribe((res) => {
      this.getTasks();
    });
  }

  putTask(identifier: string, name: string, priority: number, status: number) {
    const body = { 'name': name, 'priority': priority, 'status': status };
    this.http.put("https://localhost:5001/v1/Tasks/" + identifier, body, { 'headers': { 'content-type': 'application/json' }}).subscribe((res) => {
      this.getTasks();
    });
  }

  removeTask(identifier: string) {
    this.http.delete("https://localhost:5001/v1/Tasks/" + identifier).subscribe((res) => {
      this.getTasks();
    });
  }

  ngOnInit() {
    this.getTasks();
  }
}
