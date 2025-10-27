#!/bin/bash

# Script to fix all domain events to properly implement IDomainEvent interface

# List of event files that need fixing
EVENT_FILES=(
    "src/VirtualQueue.Domain/Events/UserActivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueCreatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueDeactivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueScheduleUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UsersReleasedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserEmailVerifiedEvent.cs"
    "src/VirtualQueue.Domain/Events/TenantCreatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateActivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserSuspendedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserProfileUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserTwoFactorDisabledEvent.cs"
    "src/VirtualQueue.Domain/Events/UserServedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationCancelledEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationFailedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserPhoneVerifiedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationProgressUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationCompletedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateCreatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserTwoFactorEnabledEvent.cs"
    "src/VirtualQueue.Domain/Events/UserDroppedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserLoggedInEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateVisibilityChangedEvent.cs"
    "src/VirtualQueue.Domain/Events/TenantDeactivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationCreatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateDeactivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserRoleUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserEmailUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserReleasedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueActivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserPasswordUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateUsedEvent.cs"
    "src/VirtualQueue.Domain/Events/TenantActivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserEnqueuedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserDeactivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationStartedEvent.cs"
)

# Function to fix a single event file
fix_event_file() {
    local file="$1"
    echo "Fixing $file..."
    
    # Check if file exists and contains IDomainEvent
    if [[ -f "$file" ]] && grep -q "IDomainEvent" "$file"; then
        # Check if it already has the Id and OccurredOn properties
        if ! grep -q "public Guid Id" "$file"; then
            # Replace the record syntax with proper implementation
            sed -i.bak 's/); : IDomainEvent;/) : IDomainEvent\n{\n    public Guid Id { get; } = Guid.NewGuid();\n    public DateTime OccurredOn { get; } = DateTime.UtcNow;\n}/' "$file"
            rm -f "$file.bak"
            echo "Fixed $file"
        else
            echo "Already fixed $file"
        fi
    else
        echo "Skipping $file (not found or doesn't implement IDomainEvent)"
    fi
}

# Fix all event files
for file in "${EVENT_FILES[@]}"; do
    fix_event_file "$file"
done

echo "All domain events have been fixed!"
