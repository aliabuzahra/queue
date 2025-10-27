#!/usr/bin/env python3

import os
import re

# List of files that need fixing
files_to_fix = [
    "src/VirtualQueue.Application/Commands/Queues/MergeQueuesCommandHandler.cs",
    "src/VirtualQueue.Application/Commands/Users/CreateUserCommandHandler.cs",
    "src/VirtualQueue.Application/Commands/Users/UpdateUserProfileCommandHandler.cs",
    "src/VirtualQueue.Application/Queries/Dashboard/GetAdminDashboardQueryHandler.cs",
    "src/VirtualQueue.Application/Queries/Users/GetUserByIdQueryHandler.cs",
    "src/VirtualQueue.Application/Queries/Users/GetUsersByTenantQueryHandler.cs"
]

def fix_handler_file(file_path):
    """Fix a handler file to include the DTOs using statement."""
    print(f"Fixing {file_path}...")
    
    if not os.path.exists(file_path):
        print(f"File not found: {file_path}")
        return False
    
    # Read the file
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Check if already has the DTOs using statement
    if "using VirtualQueue.Application.DTOs;" in content:
        print(f"Already fixed: {file_path}")
        return True
    
    # Add the DTOs using statement after the existing using statements
    pattern = r'(using VirtualQueue\.Application\.Common\.Interfaces;\n)'
    replacement = r'\1using VirtualQueue.Application.DTOs;\n'
    
    # Replace the pattern
    new_content = re.sub(pattern, replacement, content)
    
    if new_content != content:
        # Write the fixed content
        with open(file_path, 'w') as f:
            f.write(new_content)
        print(f"Successfully fixed: {file_path}")
        return True
    else:
        print(f"No changes needed: {file_path}")
        return False

# Fix all files
success_count = 0
for file_path in files_to_fix:
    if fix_handler_file(file_path):
        success_count += 1

print(f"\nFixed {success_count} out of {len(files_to_fix)} files")
print("All handler files have been processed!")
