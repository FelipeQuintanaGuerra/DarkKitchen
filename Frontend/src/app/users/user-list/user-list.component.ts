import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { UserService } from '../../services/user.service';
import { UserSummary } from '../../models/user.model';

@Component({
  selector: 'app-user-list',
  standalone: false,
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit, OnDestroy {
  users: UserSummary[] = [];
  allUsers: UserSummary[] = [];
  searchName: string = '';
  searchSurname: string = '';
  errorMessage: string | null = null;

  private roleMap: { [key: number]: string } = {
    0: 'Client',
    1: 'Dispatcher',
    2: 'Admin'
  };

  private searchSubscription: Subscription | null = null;
  private deleteSubscription: Subscription | null = null;

  getRoleName(role: any): string {
    if (typeof role === 'number') {
      return this.roleMap[role] || 'Desconocido';
    }
    return role || 'Desconocido';
  }

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    this.deleteSubscription?.unsubscribe();
  }

  loadUsers(): void {
    this.errorMessage = null;
    this.searchSubscription = this.userService.searchBy('', '').subscribe({
      next: (users) => {
        this.allUsers = users;
        this.applyFilter();
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al cargar los usuarios';
        }
      }
    });
  }

  search(): void {
    this.errorMessage = null;
    this.applyFilter();
    if (this.users.length === 0) {
      this.errorMessage = 'No se encontraron usuarios';
    }
  }

  clearSearch(): void {
    this.searchName = '';
    this.searchSurname = '';
    this.errorMessage = null;
    this.applyFilter();
  }

  private applyFilter(): void {
    const name = this.searchName.trim().toLowerCase();
    const surname = this.searchSurname.trim().toLowerCase();

    this.users = this.allUsers.filter(u =>
      (name === '' || u.name.toLowerCase().includes(name)) &&
      (surname === '' || u.surname.toLowerCase().includes(surname))
    );
  }

  deleteUser(id: string): void {
    if (!confirm('¿Estás seguro de que querés eliminar este usuario?')) {
      return;
    }

    this.deleteSubscription = this.userService.delete(id).subscribe({
      next: () => {
        this.allUsers = this.allUsers.filter(u => u.id !== id);
        this.applyFilter();
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al eliminar usuario';
        }
      }
    });
  }
}
