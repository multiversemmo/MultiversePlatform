

#include <errno.h> /* errno */
#include <fcntl.h> /* fcntl, open */
#include <stdio.h> /* perror, fprintf, stderr, printf */
#include <stdlib.h> /* exit, calloc, free */
#include <string.h> /* strerror */
#include <sys/stat.h> /* stat, fstat */
#include <sys/types.h> /* size_t */
#include <unistd.h> /* sysconf, close */


main(int argc, char* argv[])
{
    int fd, rc, ii, arg;
    int advise = POSIX_FADV_DONTNEED;

    for (arg = 1; arg < argc; arg++) {
        if (argv[arg][0] != '-')
            break;
        if (strcmp(argv[arg], "--sequential") == 0)
            advise = POSIX_FADV_SEQUENTIAL;
        else if (strcmp(argv[arg], "--random") == 0)
            advise = POSIX_FADV_RANDOM;
        else if (strcmp(argv[arg], "--willneed") == 0)
            advise = POSIX_FADV_WILLNEED;
        else if (strcmp(argv[arg], "--dontneed") == 0)
            advise = POSIX_FADV_DONTNEED;
        else if (strcmp(argv[arg], "--normal") == 0)
            advise = POSIX_FADV_NORMAL;
        else
            fprintf(stderr, "Unknown option '%s'\n", argv[arg]);
    }

    for (ii = arg; ii < argc; ii++) {
        fd = open(argv[ii], O_RDONLY);
        if (fd == -1) {
            perror(argv[ii]);
            continue;
        }
        fsync(fd);
        rc = posix_fadvise(fd, 0, 0, advise);
        if (rc != 0)
            perror("posix_fadvise");
        close(fd);
    }
    return 0;
}


