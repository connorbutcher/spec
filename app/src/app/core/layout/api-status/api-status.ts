import { httpResource } from '@angular/common/http';
import { Component, computed } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { ApiHealthState } from './api-health-state';

/** Shows whether the API (and, through its health check, the database) is reachable. */
@Component({
  selector: 'app-api-status',
  imports: [TagModule],
  templateUrl: './api-status.html',
  styleUrl: './api-status.scss',
})
export class ApiStatus {
  public readonly status = computed<ApiHealthState>(() => {
    if (this.health.isLoading()) {
      return 'checking';
    }
    return this.health.hasValue() && this.health.value() === 'Healthy' ? 'online' : 'offline';
  });

  public readonly label = computed(() => {
    switch (this.status()) {
      case 'checking': {
        return 'Checking API…';
      }
      case 'online': {
        return 'API connected';
      }
      default: {
        return 'API unavailable';
      }
    }
  });

  public readonly icon = computed(() => {
    switch (this.status()) {
      case 'checking': {
        return 'pi-spin pi-spinner';
      }
      case 'online': {
        return 'pi-check-circle';
      }
      default: {
        return 'pi-exclamation-circle';
      }
    }
  });

  public readonly severity = computed<'secondary' | 'success' | 'danger'>(() => {
    switch (this.status()) {
      case 'checking': {
        return 'secondary';
      }
      case 'online': {
        return 'success';
      }
      default: {
        return 'danger';
      }
    }
  });

  private readonly health = httpResource.text(() => '/api/health');
}
