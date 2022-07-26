import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseURl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles() {
    return this.http.get<Partial<User[]>>(this.baseURl + 'admin/users-with-roles');
  }

  getPhotosToModerate() {
    return this.http.get<Photo[]>(this.baseURl + 'admin/photos-to-moderate');
  }

  approvePhoto(photoId: number) {
    return this.http.put(this.baseURl +'admin/approve-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseURl +'admin/delete-photo/' + photoId);
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post(this.baseURl + 'admin/edit-roles/' + username + '?roles=' + roles, {})
  }
}
