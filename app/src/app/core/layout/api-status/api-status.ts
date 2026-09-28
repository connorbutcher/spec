import { httpResource } from '@angular/common/http';
import { Component, computed } from '@angular/core';
import { ApiHealthState } from './api-health-state';

/** Shows whether the API (and, through its health check, the database) is reachable. */
@Component({
  selector: 'app-api-status',
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

  private readonly health = httpResource.text(() => '/api/health');
}
