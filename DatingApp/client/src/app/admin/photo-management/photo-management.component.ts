import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Pagination } from 'src/app/_models/pagination';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[];
  pagination: Pagination;
  pageNumber = 1;
  pageSize = 5;

  constructor(private adminService: AdminService,
    private toastr: ToastrService) { }

  ngOnInit(): void {
    this.getUnapprovedPhotos();
  }

  getUnapprovedPhotos() {
    this.adminService.getUnapprovedPhotos(this.pageNumber, this.pageSize)
    .subscribe(result => {
      this.photos = result.result;
      this.pagination = result.pagination;
    });
  }

  approve(photo: Photo) {
    this.adminService.approvePhoto(photo.id).subscribe(() => {
      this.toastr.success('Photo approved');
      this.getUnapprovedPhotos();
    });
  }

  delete(photo: Photo) {
    this.adminService.deletePhoto(photo.id).subscribe(() => {
      this.toastr.success('Photo deleted');
      this.getUnapprovedPhotos();
    });
  }

  pageChanged(event) {
    this.pageNumber = event.page;
    this.getUnapprovedPhotos();
  }

}
