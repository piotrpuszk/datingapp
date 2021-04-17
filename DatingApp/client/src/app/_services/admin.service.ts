import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsersWithRoles() {
    return this.http.get<Partial<User[]>>(
      this.baseUrl + 'admin/users-with-roles'
    );
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post(
      this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles,
      {}
    );
  }

  getUnapprovedPhotos(pageNumber: number, pageSize: number) {
    const paginationParams = getPaginationHeaders(pageNumber, pageSize);
    return getPaginatedResult<Photo[]>(
      this.baseUrl + 'admin/photos-to-moderate',
      paginationParams,
      this.http
    );
  }

  approvePhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'admin/approve-photo?photoId=' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'admin/delete-photo?photoId=' + photoId);
  }
}
